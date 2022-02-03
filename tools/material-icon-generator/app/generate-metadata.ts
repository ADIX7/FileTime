import { fileIcons } from './material/icons/fileIcons';

for (var icon of fileIcons.icons!) {
  if (!icon.enabledFor && icon.fileExtensions) {
    for (var ext of icon.fileExtensions) {
      console.log('_iconsByExtension.Add("' + ext + '", "' + icon.name + '");');
    }
  }
}
for (var icon of fileIcons.icons!) {
  if (!icon.enabledFor && icon.fileNames) {
    for (var ext of icon.fileNames) {
      console.log('_iconsByFileName.Add("' + ext + '", "' + icon.name + '");');
    }
  }
}
