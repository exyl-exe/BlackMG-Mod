using BlackMGMod.MemoryAccessing;
using Iced.Intel;
using System;
using System.Collections.Generic;
using System.IO;
using static Iced.Intel.AssemblerRegisters;

namespace BlackMGMod
{
    class Program
    {
        const int MUSIC_ID = 530913;
        const int REPLACE_BY_LEVEL_ADDR = 0x1FC2BA;
        const int ANTICHEAT_JUMP_ADDR = 0x1FC352;
        const int END_OF_ANTICHEAT_ADDR = 0x1FC401;
        const int OFFICIAL_MUSIC_IF_ADDR = 0xBDCBA;
        const int MUSIC_CHOICE_ADDR = 0xBDCC1;
        const string levelFileName = "jusdebranle.blackmg";
        const string providedMusicFile = "oceandebranle.blackmg";

        static void Main(string[] args)
        {
            if (API.Initialize())
            {
                DoTheThing();
                API.RevertAllAlterations();
            } else
            {
                OnError();
            }
        }

        static void OnError()
        {
            Console.WriteLine("GD process couldn't be found, press anything to exit");
            Console.ReadKey();
        }

        static void DoTheThing()
        {
            try {
                var level = GetLevelAsBytes();
                var levelAddr = InjectLevel(level);
                CopyMusicFile();
                InjectReplaceLevelCode(levelAddr);
                InjectRiggedMusicCheck();
                InjectReplaceMusicCode();
                PatchLoadFailed();
                Console.WriteLine("BlackMG mod successfully loaded. Press any key to unload the mod.");
                Console.ReadKey();
            }
            catch (Exception)
            {
                Console.WriteLine("Pas de chance.");
                Console.ReadKey();
            }
        }

        static byte[] GetLevelAsBytes()
        {
            if (File.Exists(levelFileName))
            {
                var levelFile = File.OpenText(levelFileName);
                var res = new List<byte>();
                while (!levelFile.EndOfStream)
                {
                    char currChar = (char)levelFile.Read();
                    res.Add(BitConverter.GetBytes(currChar)[0]);//Might be completely useless
                }
                levelFile.Close();
                return res.ToArray();
            }
            else
            {
                throw new Exception();
            }
        }

        static IntPtr InjectLevel(byte[] level)
        {
            var addr = API.Accesser.AllocateMemory(level.Length);
            API.Accesser.WriteBytes(addr, level);
            return addr;
        }

        static void CopyMusicFile()
        {
            if (!File.Exists(providedMusicFile)) throw new Exception();

            var musicDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GeometryDash\\");
            if (Directory.Exists(musicDirectoryPath))
            {
                var levelMusicFileName = GetMusicFileName();
                var levelMusicFilePath = Path.Combine(musicDirectoryPath, levelMusicFileName);
                if (!File.Exists(levelMusicFilePath))
                {
                    File.Copy(providedMusicFile, levelMusicFilePath);
                }
            }
        }

        static string GetMusicFileName()
        {
            return MUSIC_ID + ".mp3";
        }

        static void InjectReplaceLevelCode(IntPtr cpAddr)
        {
            var injectAddress = API.Accesser.GetMainModuleAdress() + REPLACE_BY_LEVEL_ADDR;
            var asm = new Assembler(32);
            asm.mov(edx, (int)cpAddr);

            var stream = new MemoryStream();
            var writer = new StreamCodeWriter(stream);
            asm.Assemble(writer, 0);
            var newCode = stream.ToArray();
            stream.Close();

            API.Accesser.InsertCode(newCode, injectAddress);
        }

        static void InjectRiggedMusicCheck()
        {
            var injectAddress = API.Accesser.GetMainModuleAdress() + OFFICIAL_MUSIC_IF_ADDR;
            var newCode = new byte[0];//Nothing because it's deleting the jump to official music loading, resulting in always choosing to load a custom music
            API.Accesser.ReplaceCode(newCode, injectAddress, 2);
        }

        static void InjectReplaceMusicCode()
        {
            var injectAddress = API.Accesser.GetMainModuleAdress() + MUSIC_CHOICE_ADDR;
            var asm = new Assembler(32);
            asm.push(MUSIC_ID);

            var stream = new MemoryStream();
            var writer = new StreamCodeWriter(stream);
            asm.Assemble(writer, 0);
            var newCode = stream.ToArray();
            stream.Close();

            API.Accesser.ReplaceCode(newCode, injectAddress, 6);
        }

        static void PatchLoadFailed()
        {
            var injectAddress = API.Accesser.GetMainModuleAdress() + ANTICHEAT_JUMP_ADDR;
            var absoluteDestination = API.Accesser.GetMainModuleAdress() + END_OF_ANTICHEAT_ADDR;
            var relativeDestination = (int)absoluteDestination - (int)injectAddress;

            var asm = new Assembler(32);
            asm.jmp((ulong)relativeDestination);

            var stream = new MemoryStream();
            var writer = new StreamCodeWriter(stream);
            asm.Assemble(writer, 0);
            var newCode = stream.ToArray();
            stream.Close();

            API.Accesser.ReplaceCode(newCode, injectAddress, 6);//énorme jus de branle
        }

        
    }
}
