using System.Text;
using FileTime.App.Core.Models;
using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using InitableService;
using PropertyChanged.SourceGenerator;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;

namespace FileTime.App.Core.ViewModels.ItemPreview;

public partial class ElementPreviewViewModel : IElementPreviewViewModel, IAsyncInitable<IElement>
{
    private readonly IContentAccessorFactory _contentAccessorFactory;
    public const string PreviewName = "ElementPreview";

    private record EncodingResult(char BinaryChar, string PartialResult);

    private const int MaxTextPreviewSize = 1024 * 1024;

    private static readonly List<Encoding> Encodings = new()
    {
        Encoding.UTF8,
        Encoding.Unicode,
        Encoding.ASCII,
        Encoding.UTF32,
        Encoding.BigEndianUnicode
    };

    public ItemPreviewMode Mode { get; private set; }

    [Notify] private string _textContent = string.Empty;
    [Notify] private byte[] _binaryContent = Array.Empty<byte>();
    [Notify] private string _textEncoding = string.Empty;

    public string Name => PreviewName;

    public ElementPreviewViewModel(IContentAccessorFactory contentAccessorFactory)
    {
        _contentAccessorFactory = contentAccessorFactory;
    }

    public async Task InitAsync(IElement element)
    {
        try
        {
            if (element.FullName?.Path.EndsWith(".pdf") ?? false)
            {
                var readerFactory = _contentAccessorFactory.GetContentReaderFactory(element.Provider);
                var reader = await readerFactory.CreateContentReaderAsync(element);
                await using var inputStream = reader.AsStream();
                using var pdfDocument = PdfDocument.Open(inputStream);

                var contentBuilder = new StringBuilder();
                contentBuilder.AppendLine(element.Name + ", " + pdfDocument.NumberOfPages + " pages");
                foreach (var page in pdfDocument.GetPages())
                {
                    contentBuilder.AppendLine("=== Page " + page.Number + "===");

                    var words = page.GetWords();

                    var lines = words.GroupBy(x => (int)Math.Round((x.Letters[0].StartBaseLine.Y / 7.0) * 7));

                    foreach (var line in lines)
                    {
                        Word? previousWord = null;
                        foreach (var word in line.OrderBy(x => x.BoundingBox.Left))
                        {
                            if (previousWord != null)
                            {
                                var gap = word.BoundingBox.Left - previousWord.BoundingBox.Right;

                                var spaceSize = word.Letters[0].Width * 2;
                                if (gap > spaceSize)
                                {
                                    contentBuilder.Append(' ', (int)(gap / spaceSize));
                                }

                                contentBuilder.Append(word).Append(" ");
                            }
                            else
                            {
                                contentBuilder.Append(word).Append(" ");
                            }

                            previousWord = word;
                        }

                        contentBuilder.AppendLine();
                    }
                    contentBuilder.AppendLine();

                    if (contentBuilder.Length > MaxTextPreviewSize)
                        break;
                }

                TextContent = contentBuilder.ToString();
                TextEncoding = "UTF-8";
            }
            else
            {
                var content = await element.Provider.GetContentAsync(element, MaxTextPreviewSize);
                BinaryContent = content ?? Array.Empty<byte>();

                if (content is null)
                {
                    TextContent = "Could not read any data from file " + element.Name;
                }
                else
                {
                    (TextContent, var encoding) = GetNormalizedText(content);
                    TextEncoding = encoding is null
                        ? string.Empty
                        : $"{encoding.EncodingName} ({encoding.WebName})";
                }
            }
        }
        catch (Exception ex)
        {
            TextContent = $"Error while getting content of {element.FullName}. " + ex;
        }

        Mode = (TextContent?.Length ?? 0) switch
        {
            0 => ItemPreviewMode.Empty,
            _ => ItemPreviewMode.Text
        };

        (string, Encoding?) GetNormalizedText(byte[] data)
        {
            var binaryCharacter = new Dictionary<string, EncodingResult>();
            foreach (var encoding in Encodings)
            {
                var text = encoding.GetString(data);
                var binary = false;
                for (var i = 0; i < text.Length; i++)
                {
                    var c = text[i];
                    if (c < 32 && c != 9 && c != 10 && c != 13)
                    {
                        binaryCharacter[encoding.EncodingName] =
                            new EncodingResult(
                                c,
                                i == 0
                                    ? string.Empty
                                    : text.Substring(0, i - 1)
                            );

                        binary = true;
                        break;
                    }
                }

                if (binary) continue;

                return (text, encoding);
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("The following binary characters were found by encodings:");
            foreach (var binaryByEncoding in binaryCharacter)
            {
                stringBuilder.AppendLine(binaryByEncoding.Key + ": " + (int) binaryByEncoding.Value.BinaryChar);
            }

            var encodingsWithPartialResult = binaryCharacter.Where(e => !string.IsNullOrWhiteSpace(e.Value.PartialResult)).ToList();
            if (encodingsWithPartialResult.Count > 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("The following partial texts could be read by encodings:");
                foreach (var binaryByEncoding in encodingsWithPartialResult)
                {
                    var text = binaryByEncoding.Value.PartialResult;

                    stringBuilder.AppendLine(binaryByEncoding.Key);
                    stringBuilder.AppendLine(text);
                    stringBuilder.AppendLine();
                }
            }

            return (stringBuilder.ToString(), null);
        }
    }
}