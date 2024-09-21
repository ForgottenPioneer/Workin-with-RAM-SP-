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
