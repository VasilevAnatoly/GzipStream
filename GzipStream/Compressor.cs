using System;
using System.Threading;
using System.IO;
using System.IO.Compression;

namespace GzipStream
{
    class Compressor : Gzip
    {
        public Compressor(string input, string output) : base(input, output)
        {

        }

        public override void Launch()
        {
            Console.WriteLine("Compressing...\n");

            Thread reader = new Thread(Read);
            reader.Start();

            Thread[] compressors = new Thread[_threads];
            for (int i = 0; i < _threads; i++)
            {
                compressors[i] = new Thread(Compress);
                doneEvents[i] = new ManualResetEvent(false);
                compressors[i].Start(i);
            }

            Thread writer = new Thread(Write);
            writer.Start();

            WaitHandle.WaitAll(doneEvents);

            _queueWriter.Stop();

            if (!_cancelled)
            {
                Console.WriteLine("\nCompressing has been succesfully finished");
                _success = true;
            }
        }

        private void Read()
        {
            try
            {
                using (FileStream fileToBeCompressed = new FileStream(sourceFile, FileMode.Open))
                {

                    int bytesRead;
                    byte[] lastBuffer;

                    while (fileToBeCompressed.Position < fileToBeCompressed.Length && !_cancelled)
                    {
                        if (fileToBeCompressed.Length - fileToBeCompressed.Position <= blockSize)
                        {
                            bytesRead = (int)(fileToBeCompressed.Length - fileToBeCompressed.Position);
                        }

                        else
                        {
                            bytesRead = blockSize;
                        }

                        lastBuffer = new byte[bytesRead];
                        fileToBeCompressed.Read(lastBuffer, 0, bytesRead);

                        _queueReader.EnqueueForCompressing(lastBuffer);
                        ConsoleProgress.ProgressBar(fileToBeCompressed.Position, fileToBeCompressed.Length);
                    }
                    _queueReader.Stop();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _cancelled = true;
            }

        }

        private void Compress(object i)
        {
            try
            {
                ManualResetEvent doneEvent = doneEvents[(int)i];
                while (!_cancelled)
                {
                    ByteBlock _block = _queueReader.Dequeue();

                    if (_block == null)
                    {
                        doneEvent.Set();
                        return;
                    }

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (GZipStream cs = new GZipStream(memoryStream, CompressionMode.Compress))
                        {

                            cs.Write(_block.Buffer, 0, _block.Buffer.Length);
                        }

                        byte[] compressedData = memoryStream.ToArray();
                        ByteBlock _out = new ByteBlock(_block.ID, compressedData);

                        _queueWriter.EnqueueForWriting(_out);
                    }

                }
                doneEvent.Set();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in thread number {0}. \n Error description: {1}", i, ex.Message);
                _cancelled = true;
            }
        }

        private void Write()
        {
            try
            {
                using (FileStream fileCompressed = new FileStream(destinationFile, FileMode.Append))
                {
                    while (!_cancelled)
                    {
                        ByteBlock _block = _queueWriter.Dequeue();
                        if (_block == null)
                            return;

                        BitConverter.GetBytes(_block.Buffer.Length).CopyTo(_block.Buffer, 4);
                        fileCompressed.Write(_block.Buffer, 0, _block.Buffer.Length);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _cancelled = true;
            }
        }
    }
}