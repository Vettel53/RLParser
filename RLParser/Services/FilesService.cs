using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RLParser.Services
{
    public class FilesService
    {
        private readonly TopLevel _target;

        public FilesService(TopLevel target)
        {
            _target = target;
        }

        public async Task<IStorageFile?> OpenFileAsync()
        {
            var files = await _target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open File to Upload",
                AllowMultiple = false
            });

            return files.Count >= 1 ? files[0] : null;
        }

    }
}
