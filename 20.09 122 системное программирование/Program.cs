//using System;
//using System.Diagnostics;
//using System.Reflection.Metadata.Ecma335;
//using System.Threading;

//class Program
//{
//    static bool running = true;

//    static void Main()
//    {
//        int choice = Convert.ToInt32(Console.ReadLine());
//        switch (choice){
//            case 1:
//                Monitoring();
//                break;
//            case 2:
//                GetMemory();
//                break;

//        }
//    }
//    static void GetMemory()
//    {
//        ulong totalPhysMem = (ulong)System.Environment.WorkingSet / 1024 / 1024;
//        Console.WriteLine($"Общее количество памяти: {totalPhysMem} MB");
//    }
//    static void Monitoring()
//    {
//        ConsoleKey exitKey = ConsoleKey.Escape;


//        Thread keyPressThread = new Thread(() =>
//        {
//            while (true)
//            {
//                ConsoleKeyInfo cki = Console.ReadKey(true);
//                if (cki.Key == exitKey)
//                {
//                    running = false;
//                    break;
//                }
//            }
//        });
//        keyPressThread.Start();

//        while (running)
//        {
//            Process[] processes = Process.GetProcesses();
//            PrintAllProcesses(processes);
//            System.Threading.Thread.Sleep(1000);
//        }
//    }

//    static void PrintAllProcesses(Process[] processes)
//    {
//        Console.Clear();
//        foreach (Process process in processes)
//        {
//            try
//            {
//                Console.WriteLine($"Name: {process.ProcessName}  ID: {process.Id}  memory: {process.PagedMemorySize64 / 1024 / 1024} MB");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error accessing process {process.ProcessName}: {ex.Message}");
//            }
//        }
//        Console.WriteLine("Press Esc to exit...");
//    }
//}






using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace MemoryManagementApp
{
    class Program
    {
        public static void Main()
        {
            Console.WriteLine("Выберите действие:");
            Console.WriteLine("1. Анализ использования памяти");
            Console.WriteLine("2. Выделение и освобождение памяти");
            Console.WriteLine("3. Мониторинг использования памяти процессом");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    DisplayMemoryInfo();
                    break;
                case "2":
                    AllocateAndFreeMemory();
                    break;
                case "3":
                    MonitorProcessMemory();
                    break;
                default:
                    Console.WriteLine("Некорректный ввод");
                    break;
            }
        }

        // 1. Анализ использования памяти
        static void DisplayMemoryInfo()
        {
            long totalMemory = GetTotalPhysicalMemory();
            long freeMemory = GetAvailablePhysicalMemory();

            Console.WriteLine($"Общая память: {totalMemory} MB");
            Console.WriteLine($"Свободная память: {freeMemory} MB");
            Console.WriteLine($"Занятая память: {totalMemory - freeMemory} MB");
        }

        private static long GetTotalPhysicalMemory()
        {
            var memoryStatus = new MEMORYSTATUSEX();
            GlobalMemoryStatusEx(memoryStatus);
            return (long)(memoryStatus.ullTotalPhys / (1024 * 1024));
        }

        private static long GetAvailablePhysicalMemory()
        {
            var memoryStatus = new MEMORYSTATUSEX();
            GlobalMemoryStatusEx(memoryStatus);
            return (long)(memoryStatus.ullAvailPhys / (1024 * 1024));
        }

        // 2. Выделение и освобождение памяти
        static void AllocateAndFreeMemory()
        {
            Console.WriteLine("Введите размер блока памяти для выделения (в MB):");
            if (int.TryParse(Console.ReadLine(), out int sizeMB))
            {
                try
                {
                    byte[] memoryBlock = new byte[sizeMB * 1024 * 1024];
                    Random rand = new Random();
                    for (int i = 0; i < memoryBlock.Length; i++)
                    {
                        memoryBlock[i] = (byte)rand.Next(0, 256);
                    }
                    Console.WriteLine($"Выделено {sizeMB} MB памяти и инициализировано случайными данными.");

                    memoryBlock = null;
                    GC.Collect();
                    Console.WriteLine("Память освобождена.");
                }
                catch (OutOfMemoryException)
                {
                    Console.WriteLine("Ошибка: Недостаточно памяти для выделения.");
                }
            }
            else
            {
                Console.WriteLine("Некорректный ввод.");
            }
        }

        // 3. Мониторинг использования памяти процессом
        static void MonitorProcessMemory()
        {
            Console.WriteLine("Введите PID процесса для мониторинга:");
            if (int.TryParse(Console.ReadLine(), out int pid))
            {
                try
                {
                    Process process = Process.GetProcessById(pid);

                    while (!process.HasExited)
                    {
                        Console.WriteLine($"Использование памяти процессом {process.ProcessName}: {process.WorkingSet64 / (1024 * 1024)} MB");
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Некорректный PID.");
            }
        }

        // Импорт функции GlobalMemoryStatusEx из kernel32.dll
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx([In] MEMORYSTATUSEX lpBuffer);

        // Структура для хранения информации о памяти
        [StructLayout(LayoutKind.Sequential)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }
    }
}