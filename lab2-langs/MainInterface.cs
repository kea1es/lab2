using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Globalization;



namespace WeatherJournalApp
{
    class MainInterface
    {

        private static Weather DbContext;

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddDbContext<Weather>(options =>
                    options.UseNpgsql(configuration.GetConnectionString("PostgresConnection")))
                .BuildServiceProvider();

            try
            {
                DbContext = serviceProvider.GetRequiredService<Weather>();

                await DbContext.Database.EnsureDeletedAsync();
                await DbContext.Database.EnsureCreatedAsync();

                int choice;
                do
                {
                    choice = Choice();
                    switch (choice)
                    {
                        case 1:
                            await AddNewElem();
                            break;
                        case 2:
                            await RemoveElemeByDateAndMonth();
                            break;
                        case 3:
                            await FindElemByMonth();
                            break;
                        case 4:
                            Console.WriteLine("Выход из программы.");
                            break;

                        default:
                            Console.WriteLine("Ошибка! Можно ввести только значения {1, 2, 3, 4}.");
                            break;
                    }
                } while (choice != 4);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"\nПроизошла ошибка: {exception.Message}");
            }

        }


        static int Choice()
        {
            Console.WriteLine("Меню");
            Console.WriteLine("1. Добавить новую запись;");
            Console.WriteLine("2. Удалить записи (по дате);");
            Console.WriteLine("3. Показать данные (по месяцу);");
            Console.WriteLine("4. Выход из программы.");
            Console.Write("\nВыберите один пункт: ");

            if (int.TryParse(Console.ReadLine(), out int number)) return number;

            return 0;
        }


        static async Task AddNewElem()
        {
            Console.WriteLine("\nДобавление новой записи");

            DateTime chosenDate;
            float temp, pressure;
            string chosenType;


            Console.Write("Введите дату и время в формате (YYYY-MM-DD HH:MM): ");
            if (!DateTime.TryParse(Console.ReadLine(), CultureInfo.InvariantCulture, DateTimeStyles.None, out chosenDate))
            {
                Console.WriteLine("Формат некорректен.");
                return;
            }



            Console.Write("Введите значение температуры (в градусах Цельсия): ");

            if (!float.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out temp)) { Console.WriteLine("Некорректный формат температуры."); return; }
            Console.Write("Введите значение давление (мм рт. ст.): ");

            if (!float.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out pressure)) { Console.WriteLine("Некорректный формат давления."); return; }
            Console.Write("Введите тип осадков: ");

            chosenType = Console.ReadLine()?.Trim() ?? "Нет";


            var nowDate = DateTime.SpecifyKind(chosenDate, DateTimeKind.Local);
            var newRecord = new WeatherRec { DateOfRecord = nowDate, Temperature = temp, Pressure = pressure, Type = chosenType };

            await DbContext.Records.AddAsync(newRecord);
            await DbContext.SaveChangesAsync();

            Console.WriteLine($"\nЗапись была успешно добавлена: {newRecord.DateOfRecord:yyyy-MM-dd HH:mm}\n");
        }



        static async Task RemoveElemeByDateAndMonth()
        {
            Console.Write("Введите дату для удаления (YYYY-MM-DD): ");

            if (!DateTime.TryParse(Console.ReadLine(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                Console.WriteLine("\nФормат некорректен\n");
                return;
            }


            var recordsToDelete = await DbContext.Records
                .Where(r => r.DateOfRecord.Date == date.Date)
                .ToListAsync();

            if (recordsToDelete.Any())
            {
                DbContext.Records.RemoveRange(recordsToDelete);
                int count = await DbContext.SaveChangesAsync();
                Console.WriteLine($"\nБыло удалено {count} записей за дату: {date:yyyy-MM-dd}.\n");
            }
            else
            {
                Console.WriteLine($"\nНи одной записи за дату {date:yyyy-MM-dd} не было найдено.\n");
            }

        }

        static async Task FindElemByMonth()
        {

            Console.Write("Введите месяц (от 1 до 12): ");
            if (!int.TryParse(Console.ReadLine(), out int month) || month < 1 || month > 12) return;


            var results = await DbContext.Records
                .Where(r => r.DateOfRecord.Month == month)
                .OrderBy(r => r.DateOfRecord)
                .ToListAsync();


            Console.WriteLine($"\nДанные за {month:00} ({results.Count} записей)\n");
            if (results.Any())
            {
                foreach (var record in results)
                {
                    Console.WriteLine(record.ToString());
                }
            }
            else
            {
                Console.WriteLine("Записей найдено не было.\n");
            }

        }

    }

}