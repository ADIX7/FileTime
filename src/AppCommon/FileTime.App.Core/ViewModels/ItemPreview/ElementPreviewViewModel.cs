using System.Text;
using FileTime.App.Core.Models;
using FileTime.Core.Models;
using InitableService;
using MvvmGen;

namespace FileTime.App.Core.ViewModels.ItemPreview;

[ViewModel]
public partial class ElementPreviewViewModel : IElementPreviewViewModel, IAsyncInitable<IElement>
{
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

    [Property] private string? _textContent;
    [Property] private byte[]? _binaryContent;
    [Property] private string? _textEncoding;

    public string Name => PreviewName;

    public async Task InitAsync(IElement element)
    {
        try
        {
            var content = await element.Provider.GetContentAsync(element, MaxTextPreviewSize);
            BinaryContent = content;

            if (content is null)
            {
                TextContent = "Could not read any data from file " + element.Name;
            }
            else
            {
                (TextContent, var encoding) = GetNormalizedText(content);
                TextEncoding = encoding is null
                    ? null
                    : $"{encoding.EncodingName} ({encoding.WebName})";
            }
        }
        catch (Exception ex)
        {
            TextContent = $"Error while getting content of {element.FullName}. " + ex.ToString();
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