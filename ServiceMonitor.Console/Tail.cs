using System;
using System.IO;

namespace ServiceMonitor.Console
{
    class Tail
    {
        public event EventHandler<string> FileChanged;

        private FileSystemWatcher fileWatcher;

        private long lastPosition = 0;
        
        public Tail(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            string filename = Path.GetFileName(filePath);
            fileWatcher = new FileSystemWatcher(directory, filename);
            fileWatcher.Changed += FileWatcherOnChanged;
            fileWatcher.EnableRaisingEvents = true;
        }

        private void FileWatcherOnChanged(object sender, FileSystemEventArgs eventArgs)
        {
            using (FileStream inStream = new FileStream(eventArgs.FullPath, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite))
            {
                inStream.Position = lastPosition;
                using (StreamReader streamReader = new StreamReader(inStream))
                {
                    while (true)
                    {
                        string delta = streamReader.ReadLine();
                        if (delta == null)
                        {
                            break;
                        }
                        FileChanged?.Invoke(eventArgs.Name, delta);
                    }
                    lastPosition = inStream.Position;
                }
            }
        }
    }
}
