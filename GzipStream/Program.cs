using System;

namespace GzipStream
{
    internal class Program
    {
        static Gzip _zipper;

        static int Main(string[] args)
        {
            Console.CancelKeyPress += CancelKeyPress;

            ShowInfo();

            try
            {
                args = new string[3];
                //args[0] = @"compress";
                //args[1] = @"CLR.pdf";
                //args[2] = @"CLR1.gz";
                args[0] = @"decompress";
                args[1] = @"CLR1.gz";
                args[2] = @"CLR2.pdf";


                Validation.StringReadValidation(args);

                switch (args[0].ToLower())
                {
                    case "compress":
                        _zipper = new Compressor(args[1], args[2]);
                        break;
                    case "decompress":
                        _zipper = new Decompressor(args[1], args[2]);
                        break;
                }

                _zipper.Launch();
                return _zipper.CallBackResult();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error is occured!\n Method: {0}\n Error description {1}", ex.TargetSite, ex.Message);
                return 1;
            }
        }

        static void ShowInfo()
        {
            Console.WriteLine("To complete the program correct please use the combination CTRL + C");
        }


        static void CancelKeyPress(object sender, ConsoleCancelEventArgs _args)
        {
            if (_args.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                Console.WriteLine("\nCancelling...");
                _args.Cancel = true;
                _zipper.Cancel();
            }
        }
    }
}
