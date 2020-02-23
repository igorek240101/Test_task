using System;
using System.IO;

namespace Test_task
{
    class Program
    {    
        static void Main(string[] args)
        {
            int image_count = 0;
            int table_count = 0;
            Simple_struct image = new Simple_struct(new string[] { "![", "](", ")" }, new char[]{(char)3200, (char)3200, ' '}); // Структура синтаксической конструкции - "Рисунок" - вид ![...](...исключая ' ' ...)
            for (int i = 0; i < args.Length; i++) // Цикл обхода файлов
            {
                FileStream file = new FileStream(args[i], FileMode.Open);
                StreamReader reader = new StreamReader(file);
                string last = null;
                string pre_last = null;
                int table_wh_count = 0;
                bool image_was_fanding = false;
                while (!reader.EndOfStream) 
                {
                    string active = reader.ReadLine();
                    if (active.Length == 0) { active = "1";}
                    if(image_was_fanding)// Проверка имени рисунка (обнаруженного в предыдущей строке при условии окончании строки синтаксисом изображения)
                    {
                        Name_test(active, "Рисунок", image_count, args[i]);
                        image_was_fanding = false;
                    }
                    for(int j = 0; j < active.Length; j++) // Исследование строки на наличие изображения
                    {
                        if(active[j]==image.Active_fall)
                        {
                            image.Nullable();
                        }
                        if (active[j] == image.Active)
                        {
                            j++;
                            image++;
                            while(j < active.Length && !image.End_of_string)
                            {
                                if (active[j] != image.Active) { image.Nullable(); break; }
                                j++; image++;
                            }
                        }
                        if (image.End_of_Search)
                        {
                            image_count++;
                            if(j==active.Length)
                            {
                                image_was_fanding = true;
                            }
                            else
                            {
                                string s = null;
                                for (int k = j; k < active.Length; k++)
                                {
                                    s += active[k];
                                }
                                Name_test(s, "Рисунок", image_count, args[i]);
                            }
                            image.Nullable();
                        }
                    }
                    image.Nullable();
                    if (table_wh_count <= 0) // Поиск таблицы
                    {
                        if (active[0] == '|' && active[active.Length - 1] == '|') // Поиск заголовка
                        {
                            if(table_wh_count == 0)table_wh_count = active.Split('|').Length - 2;
                        }
                        else table_wh_count = 0;
                    }
                    else
                    {
                        string[] istable = active.Split('|'); // Проверка второй строки
                        if (istable.Length - 2 == table_wh_count)
                        {
                            for(int j = 1; j < istable.Length-1; j++) // Посимвольный обход столбцов
                            {
                                int first_symbol = 0;
                                for (int k = 0; k < istable[j].Length; k++)
                                {
                                    if (istable[j][k] == '-') { first_symbol = k; break; }
                                    if (istable[j][k] == ':') { first_symbol = k + 1; break; }
                                    if (istable[j][k] != ' ') { table_wh_count = -1; break; }
                                    if (k == istable[j].Length - 1) { table_wh_count = -1; break; }
                                }
                                for (int k = first_symbol; k<istable[j].Length; k++)
                                {
                                    if (istable[j][k] == ':') { first_symbol = k + 1; break; }
                                    if (istable[j][k] == ' ') { first_symbol = k; break; }
                                    if (istable[j][k] != '-') { table_wh_count = -1; break; }
                                    if (k == istable[j].Length - 1) { first_symbol = k + 1; break; }
                                }
                                for (int k = first_symbol; k < istable[j].Length; k++)
                                {
                                    if (istable[j][k] != ' ') { table_wh_count = -1; break; }
                                }

                            }
                            if (table_wh_count != -1)
                            {
                                table_count++;
                                Name_test(last, "Таблица", table_count, args[i]);
                            }
                        }
                        else { table_wh_count = -1; }
                    }
                    last = pre_last;
                    pre_last = active;
                }
                reader.Close();
                file.Close();

                Console.WriteLine(args[i] + "   Суммарное количество рисунков и таблиц  = " + (image_count + table_count) );
                image_count = 0; table_count = 0;
            }
            Console.ReadKey();
        }

        public static void Name_test(string name, string correct_name, int count, string now_file) // Метод проверки корректности именования элемента (таблицы или рисунка)
        {
            string[] str = name.Split(' ');
            if (str[0] != correct_name) Console.WriteLine(now_file + "   " + correct_name + " " + count + ". -    некорректное либо отсутствующее название "); 
            else
            {
                str = str[1].Split('.');
                if (str.Length < 2) Console.WriteLine(now_file + "   " + correct_name + " " + count + ". -    некорректное либо отсутствующее название ");
                int index = 0;
                try
                {
                    index = Convert.ToInt32(str[0]);
                }
                catch { Console.WriteLine(now_file + "   " + correct_name + " " + count + ". -    некорректное либо отсутствующее название "); return; }
                if (index != count) 
                {
                    Console.WriteLine(now_file + "   " + correct_name + " " + index + ". -   неправильно проиндексирован, ожидалось " + correct_name + " " + count + "."); 
                }
            }
        }

    }


    class Simple_struct // Класс для поиска однострочных синтаксических конструкций Markdown (также пригоден для ряда многострочных)
    {
        string[] sintax; // Критерии синтаксической конструкции по наличию подстроки
        char[] fall_symboll; //Критерий синтаксической конструкции по отсутствию символа
        int sintax_id = 0;              
        int sintax_pre_id = 0; // Индексы критериев

        public Simple_struct(string[] elements, char[] fall_elements)
        {
            sintax = elements;
            fall_symboll = fall_elements;
        }

        public char Active // Возвращает текущий символ присутствия
        {
            get
            {
                return sintax[sintax_id][sintax_pre_id];
            }
        }

        public char Active_fall // Возвращает текущий символ отсутствия
        {
            get
            {
                return fall_symboll[sintax_id];
            }
        }

        public bool End_of_string // Отражает статус перемещения по подстроке
        {
            get
            {
                return sintax_pre_id == 0;
            }
        }

        public void Nullable() // Обнуляет состояние поиска
        {
            sintax_id = 0; sintax_pre_id = 0;
        }  

        public bool End_of_Search // Отражает статус поиска по строке
        {
            get
            {
                return sintax.Length <= sintax_id;
            }
        }


        public static Simple_struct operator ++(Simple_struct element) //Инкремент стадии поиска
        {
            if(++element.sintax_pre_id==element.sintax[element.sintax_id].Length)
            {
                element.sintax_pre_id = 0;
                element.sintax_id++;
            }
            return element;
            
        }

    }
}
