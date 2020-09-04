using System;
namespace BlackMGMod.MemoryAccessing
{
    static class API
    {
        //GD :heart_eyes:
        const string GDProcessName = "GeometryDash";

        //Memory values to access
        static readonly MemoryValue level = new MemoryValue(new[] { 0x3222D0, 0x164}, 4);
        static readonly MemoryValue player = new MemoryValue(new[] { 0x3222D0, 0x164, 0x224 }, 4);
        static readonly MemoryValue playerXPosition = new MemoryValue(new[] { 0x3222D0, 0x164, 0x224, 0x67C }, 4);
        static readonly MemoryValue playerIsDead = new MemoryValue(new[] { 0x3222D0, 0x164, 0x224, 0x63F }, 1);
        static readonly MemoryValue playerHasWon = new MemoryValue(new[] { 0x3222D0, 0x164, 0x224, 0x662 }, 1);
        static readonly MemoryValue playerGravity = new MemoryValue(new[] { 0x3222D0, 0x164, 0x224, 0x63E }, 1);
        static readonly MemoryValue currentSessionAttempts = new MemoryValue(new[] { 0x3222D0, 0x164, 0x4A8 }, 4);

        public static MemoryAccesser Accesser { get; private set; }

        //To check if api has been initialized
        static bool initialized;

        public static int GetLevelAddr()
        {
            APIUsableCheck();
            int bytes = 0;
            return BitConverter.ToInt32(Accesser.ReadMemoryValue(level, ref bytes), 0);
        }

        public static int GetPlayerAddr()
        {
            LevelAPIUsableCheck();
            int bytes = 0;
            return BitConverter.ToInt32(Accesser.ReadMemoryValue(player, ref bytes), 0);
        }

        public static float GetPlayerPos()
        {
            LevelAPIUsableCheck();
            int bytes = 0;
            return BitConverter.ToSingle(Accesser.ReadMemoryValue(playerXPosition, ref bytes), 0);
        }

        public static int GetCurrentAttempt()
        {
            LevelAPIUsableCheck();
            int bytes = 0;
            return BitConverter.ToInt32(Accesser.ReadMemoryValue(currentSessionAttempts, ref bytes), 0);
        }

        public static bool IsGravityUpward()
        {
            LevelAPIUsableCheck();
            int bytes = 0;
            return Accesser.ReadMemoryValue(playerGravity, ref bytes)[0] == 0x01;
        }

        public static bool IsInLevel()
        {
            APIUsableCheck();
            return GetLevelAddr() != 0;
        }

        public static bool HasPlayerWon()
        {
            LevelAPIUsableCheck();
            int bytes = 0;
            return Accesser.ReadMemoryValue(playerHasWon, ref bytes)[0] == 0x1;//TODO mes meilleurs constantes
        }

        public static bool IsPlayerDead()
        {
            LevelAPIUsableCheck();
            int bytes = 0;
            return Accesser.ReadMemoryValue(playerIsDead, ref bytes)[0] == 0x1;
        }

        public static bool Initialize()
        {
            Accesser = new MemoryAccesser();     
            var success = Accesser.AttachTo(GDProcessName);
            if (success)
            {
                initialized = true;
            }
            return success;
        }

        public static void RevertAllAlterations()
        {
            Accesser.ResetTargetProcessMemory();
            initialized = false;
        }

        public static bool IsInitialized()
        {
            return initialized;
        }

        public static void APIUsableCheck()
        {
            if (!initialized)
            {
                throw new Exception("GD API was used without being initialized");
            }

            if (Accesser.process.HasExited)
            {
                throw new Exception("GD API was used but GD process has been terminated");
            }
        }

        public static void LevelAPIUsableCheck()
        {
            APIUsableCheck();
            if (!IsInLevel())
            {
                throw new Exception("GD API was used while not playing a level");
            }
        }
    }
}
