using Mao.Generate.Core.Interfaces;
using Mao.Generate.Core.Models;
using Mao.Generate.Core.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Services
{
    public class GenerateService
    {
        /// <summary>
        /// 取得目錄中相對路徑的目錄物件
        /// </summary>
        private WinDirectory Switch(WinDirectory root, string path)
        {
            path = path?.Trim('/', '\\');
            if (string.IsNullOrWhiteSpace(path))
            {
                return root;
            }
            var directoryNames = path.Split('/', '\\');
            WinDirectory current = root;
            foreach (var directoryName in directoryNames)
            {
                var directory = current.Directories.FirstOrDefault(x => x.Name == directoryName);
                if (directory == null)
                {
                    directory = new WinDirectory()
                    {
                        Name = directoryName,
                        Directories = new List<WinDirectory>(),
                        Files = new List<WinTextFile>()
                    };
                    current.Directories.Add(directory);
                }
                current = directory;
            }
            return current;
        }
        /// <summary>
        /// 透過 IGenerator 將參數轉換成檔案
        /// </summary>
        public WinDirectory GenerateFiles<TInput>(IGenerator<TInput> generator, TInput input, GenerateFilesOptions options = null)
        {
            WinDirectory directory = new WinDirectory()
            {
                Directories = new List<WinDirectory>(),
                Files = new List<WinTextFile>()
            };
            var methods = generator.GetType().GetMethods()
                .Where(x => typeof(GeneratedText).IsAssignableFrom(x.ReturnType) || typeof(IEnumerable<GeneratedText>).IsAssignableFrom(x.ReturnType))
                .Where(x => x.GetParameters().Length == 1)
                .Where(x => options == null
                    || ((options.ExecuteMethodNames?.Length ?? 0) + (options.IgnoreMethodNames?.Length ?? 0) == 0)
                    || (options.ExecuteMethodNames != null && options.ExecuteMethodNames.Contains(x.Name, StringComparison.OrdinalIgnoreCase))
                    || ((options.ExecuteMethodNames?.Length ?? 0) == 0 && (options.IgnoreMethodNames?.Length ?? 0) != 0 && !options.IgnoreMethodNames.Contains(x.Name, StringComparison.OrdinalIgnoreCase)))
                .ToArray();
            List<GeneratedText> generateds = new List<GeneratedText>();
            foreach (var method in methods)
            {
                object methodParameter;
                var parameterType = method.GetParameters()[0].ParameterType;
                if (parameterType.IsAssignableFrom(typeof(TInput)))
                {
                    methodParameter = input;
                }
                else if (ObjectResolver.TryConvert(generator, input, parameterType, out methodParameter) == false)
                {
                    continue;
                }
                var generated = method.Invoke(generator, new object[] { methodParameter });
                if (generated is GeneratedText generatedText)
                {
                    generateds.Add(generatedText);
                }
                if (generated is IEnumerable<GeneratedText> generatedTexts)
                {
                    generateds.AddRange(generatedTexts);
                }
            }
            foreach (var generatedTextFile in generateds.Where(x => x != null))
            {
                WinTextFile file = new WinTextFile();
                if (string.IsNullOrWhiteSpace(generatedTextFile.Name))
                {
                    file.Name = (options?.DefaultNameFactory() ?? Guid.NewGuid().ToString()).ToFileName();
                }
                else
                {
                    file.Name = generatedTextFile.Name.ToFileName();
                }
                file.Text = generatedTextFile.Text;
                // 將檔案放置目錄中
                Switch(directory, generatedTextFile.Path).Files.Add(file);
            }
            return directory;
        }
        /// <summary>
        /// 將檔案做成壓縮檔
        /// </summary>
        public byte[] ZipFiles(WinDirectory container)
        {
            if (container != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Update, false, Encoding.UTF8))
                    {
                        Action<string, WinDirectory> resolveDirectory = null;
                        Action<string, WinTextFile> resolveFile = null;
                        resolveDirectory = (path, directory) =>
                        {
                            if (directory.Directories != null && directory.Directories.Any())
                            {
                                foreach (var child in directory.Directories)
                                {
                                    resolveDirectory(
                                        string.IsNullOrWhiteSpace(directory.Name) ?
                                            path :
                                            Path.Combine(path, directory.Name),
                                        child);
                                }
                            }
                            if (directory.Files != null && directory.Files.Any())
                            {
                                foreach (var file in directory.Files)
                                {
                                    resolveFile(path, file);
                                }
                            }
                        };
                        resolveFile = (path, file) =>
                        {
                            var entry = zipArchive.CreateEntry(Path.Combine(path, file.Name));
                            using (var entryStream = entry.Open())
                            {
                                var buffer = Encoding.UTF8.GetBytes(file.Text ?? string.Empty);
                                entryStream.Write(buffer, 0, buffer.Length);
                            }
                        };
                        resolveDirectory("", container);
                    }
                    return memoryStream.ToArray();
                }
            }
            return null;
        }
        /// <summary>
        /// 將檔案儲存到指定路徑
        /// </summary>
        public void ExportFiles(WinDirectory container, string exportPath)
        {
            if (container != null && !string.IsNullOrWhiteSpace(exportPath))
            {
                Action<string, WinDirectory> resolveDirectory = null;
                Action<string, WinTextFile> resolveFile = null;
                resolveDirectory = (path, directory) =>
                {
                    if (directory.Directories != null && directory.Directories.Any())
                    {
                        foreach (var child in directory.Directories)
                        {
                            resolveDirectory(
                                string.IsNullOrWhiteSpace(directory.Name) ?
                                    path :
                                    Path.Combine(path, directory.Name),
                                child);
                        }
                    }
                    if (directory.Files != null && directory.Files.Any())
                    {
                        foreach (var file in directory.Files)
                        {
                            resolveFile(path, file);
                        }
                    }
                };
                resolveFile = (path, file) =>
                {
                    Directory.CreateDirectory(path);
                    File.WriteAllText(Path.Combine(path, file.Name), file.Text, Encoding.UTF8);
                };
                resolveDirectory(exportPath, container);
            }
        }
        /// <summary>
        /// 將檔案壓縮後儲存到指定位置
        /// </summary>
        /// <param name="files"></param>
        /// <param name="zipFullPath">包含目錄與檔案名稱的完整路徑</param>
        public void ExportZip(WinDirectory directory, string zipFullPath)
        {
            if (directory != null && !string.IsNullOrWhiteSpace(zipFullPath))
            {
                int index = zipFullPath.LastIndexOfAny(new[] { '/', '\\' });
                if (index >= 0)
                {
                    var directoryPath = zipFullPath.Substring(0, index);
                    Directory.CreateDirectory(directoryPath);
                }
                var bytes = ZipFiles(directory);
                File.WriteAllBytes(zipFullPath, bytes);
            }
        }
    }
}
