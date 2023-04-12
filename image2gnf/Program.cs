using GFDLibrary.Textures;
using GFDLibrary.Textures.DDS;
using GFDLibrary.Textures.GNF;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace image2gnf
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("[Usage] image2gnf.exe input1.dds input2.dds input3.dds...");
                    Console.WriteLine("[Usage] image2gnf.exe input1.gnf input2.gnf input3.gnf...");
                    return;
                }

                foreach (string arg in args) await Export(arg);
            }
            finally
            {
                Console.ReadKey();
            }
        }

        private static async Task Export(string input)
        {
            await Task.Factory.StartNew(() => {
                try
                {
                    string inputExt = Path.GetExtension(input);
                    bool isDDS = inputExt == ".dds";
                    string outputExt = isDDS ? ".gnf" : ".dds";
                    string output = Path.GetFileNameWithoutExtension(input) + outputExt;
                    using Stream outputStream = File.OpenWrite(output);
                    Console.Write("Open {0}...", input);
                    if (isDDS)
                    {
                        using DDSStream ddsStream = new DDSStream(input);

                        GNFTexture texture = new GNFTexture(ddsStream);
                        GNFexport.ExportCore(texture, outputStream);
                        outputStream.Close();

                        // validate GNF
                        using Stream tmp1 = File.OpenRead(output);
                        if (GNFexport.CanImportCore(tmp1)) Console.WriteLine(", write GNF and valid ok");
                    }
                    else
                    {
                        GNFTexture texture = new GNFTexture(input);
                        byte[] ddsBytes = TextureDecoder.DecodeToDDS(texture);
                        using Stream stream = new MemoryStream(ddsBytes);
                        using DDSStream ddsStream = new DDSStream(stream);

                        DDSExport.ExportCore(ddsStream, outputStream);
                        outputStream.Close();

                        // validate DDS
                        using Stream tmp1 = File.OpenRead(output);
                        if (DDSExport.CanImportCore(tmp1)) Console.WriteLine(", write DDS and valid ok");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Export Exception:\n{0},\n{1}", e.StackTrace, e.ToString());
                }
            });
        }
    }
}
