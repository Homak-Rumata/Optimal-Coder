
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data.SqlTypes;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Частотный_анализ
{
    public static class StringExtensions
    {
        public static int Count(this string input, string substr)
        {
            Console.WriteLine(substr);
            return Regex.Matches(input, substr).Count;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            string command = "";

            while (true)
            {
                Console.WriteLine("пропишите quit для выхода");
                Console.WriteLine("Введите путь к файлу: ");
                command = Console.ReadLine();
                try
                {
                    if (command != "quit")
                    {
                        
                        int num;
                        Console.WriteLine("Введите размер группы кодирования: ");
                        num = Int32.Parse(Console.ReadLine());
                        Iterator a = new Iterator(File.OpenRead(command), num);
                        StreamWriter writer = new StreamWriter($"{command}-output.txt", false, Encoding.Default);

                        int summ = a.GetCode.Sum(element => element.Value.Item4);
                        var exeption = a.GetCode;
                        int stringvalue = exeption.Select(i => i.Value.Item2).Max(i => i.Length) >
                            exeption.Select(i => i.Value.Item1).Max(i => i.Length) ?
                            exeption.Select(i => i.Value.Item2).Max(i => i.Length) :
                            exeption.Select(i => i.Value.Item1).Max(i => i.Length);
                        (string, string, string, string) Tittle;
                        Tittle = ("Cимвол | ", "Кодировка по хаффману | ", "Кодировка по Шенону-Фано | ", "Улучшенная кодировка по Шенону-Фано | ");
                        Console.WriteLine($"{Tittle.Item1} {Tittle.Item2} {Tittle.Item3} {Tittle.Item4} Колличество вхождений символа");
                        foreach (KeyValuePair<string, (string, string, string, int)> item 
                            in exeption.OrderByDescending(temp => temp.Value.Item4))
                        {
                            Console.WriteLine($"{(item.Key+":").PadRight(exeption.Keys.Select(i=>i.Length).Max() >Tittle.Item1.Length - 2 ? exeption.Keys.Select(i => i.Length).Max() : Tittle.Item1.Length - 2)}" +
                                $"|{item.Value.Item1.PadRight(stringvalue > Tittle.Item2.Length - 2 ? stringvalue : Tittle.Item2.Length - 2)} |" +
                                $"|{item.Value.Item2.PadRight(stringvalue > Tittle.Item3.Length - 2 ? stringvalue : Tittle.Item3.Length - 2)} |" +
                                $"|{item.Value.Item3.PadRight(stringvalue > Tittle.Item4.Length - 2 ? stringvalue : Tittle.Item4.Length - 2)} |" +
                                $"|{item.Value.Item4}/{summ} ");
                            writer.WriteLine($"{item.Key}\t" +
                                $"{item.Value.Item1}\t" +
                                $"{item.Value.Item2}\t" +
                                $"{item.Value.Item3}\t"+
                                $"={item.Value.Item4}/{summ}");
                        };
                        double HFact = exeption.Sum(i => ((i.Value.Item4 / (double)summ) * Math.Log((double)summ / (i.Value.Item4), 2)));
                        double HIdeal = Math.Log(exeption.Count(), 2);
                        double QAverageShenonFano = exeption.Sum(i => i.Value.Item2.Length * (i.Value.Item4 / (double)summ));
                        double QUpgradeAverageShenonFano = exeption.Sum(i => i.Value.Item3.Length * (i.Value.Item4 / (double)summ)); 
                        double QAverageHafman = exeption.Sum(i => i.Value.Item1.Length * (i.Value.Item4 / (double)summ));
                        Console.WriteLine($"H(X) Фактическая = {HFact}\n" +
                            $"H(X) Идеальная = {HIdeal}\n" +
                            $"Избыточность алфавита r_алф = {((HIdeal - HFact) / HIdeal)*100}%\n\n" +
                            $"Количество символов: {exeption.Count()} \n" +
                            "Для кода Шенона-Фано: \n" +
                            $"Средняя длинна кодовго слова n(cp) = {QAverageShenonFano} \n" +
                            $"Избыточность кода r_кода {(QAverageShenonFano - HFact) / QAverageShenonFano}\n" +
                            $"Эффективность кода {HFact / QAverageShenonFano}\n\n" +
                            "Для улучшенного кода Шенона-Фано: \n" +
                            $"Средняя длинна кодовго слова n(cp) = {QUpgradeAverageShenonFano} \n" +
                            $"Избыточность кода r_кода {(QUpgradeAverageShenonFano - HFact) / QUpgradeAverageShenonFano}\n" +
                            $"Эффективность кода {HFact / QUpgradeAverageShenonFano}\n\n" +
                            "Для кода Хафмана:\n" +
                            $"Средняя длинна кодовго слова n(cp) = {QAverageHafman} \n" +
                            $"Избыточность кода r_кода {(QAverageHafman - HFact) / QAverageHafman}\n" +
                            $"Эффективность кода {HFact / QAverageHafman}");


                        writer.Close();
                    }
                    else
                    {
                        break;
                    }
                }
                catch
                {
                    Console.WriteLine("Файл не найден");
                }
            }


            
        }


    }

    public class Iterator
    {
        private string _tempString;

        private (char, double)[] _iter;

        public Dictionary<string, (string, string, string, int)> GetCode { get; private set; }

        public Iterator(FileStream file, int num)
        {
            byte[] buffer = new byte[file.Length];
            file.Read(buffer, 0, buffer.Length);

            _tempString = Encoding.UTF8.GetString(buffer);
            file.Close();
            GroupIter(_tempString.ToLower(), num);
            
        }

        private void SetIter(string _tempString)
        {
            int length = _tempString.Length;
            List<(char, int)> chars = new List<(char, int)>();

            while (_tempString.Length > 0)
            {
                char temp = _tempString[0];
                int tempInt = 0;
                Console.Clear();
                Console.WriteLine($"{temp} - {_tempString.Length}");
                try
                {
                    if (temp == '.')
                    {
                        throw new Exception("ТОЧКА");
                    }
                    chars.Add((temp, _tempString.Count(temp.ToString())));
                    _tempString = _tempString.Replace(temp.ToString(), "");
                }
                catch
                {
                    while (_tempString.IndexOf(temp) >= 0)
                    {

                        tempInt++;

                        _tempString = _tempString.Remove(_tempString.IndexOf(temp), 1);
                    }

                    chars.Add((temp, tempInt));
                }
            }
            chars.Sort(Compare);
            int ControlSumm = 0;
            Console.Clear();
            string coder = "1";
            foreach ((char, int) c in chars)
            {
                Console.Write(c.Item1);
                ControlSumm += c.Item2;
                Console.WriteLine(" - {1:f5}% {2}/{3} - {4}",
                    c.Item1, ((double)c.Item2 / length) * 100, c.Item2, length, coder);
                coder = "0" + coder;


            }


            Console.WriteLine("{0} - {1}", ControlSumm.ToString(), length);
        }

        private void GroupIter(string _tempString, int length)
        {

            Console.Clear();
            int i = 0;
            int Count()
            {
                i++;
                return i;
            }

            IEnumerable<char> GetEnum(int indexator, int counter)
            {
                var temp = _tempString.Where((_, index) => index % indexator == counter);
                return temp;
            }

            IEnumerable<string> Construct(int indexator)
            {
                return Constructor(indexator, from c in GetEnum(indexator, 0)
                                              select Filter(c), 1);
            }

            IEnumerable<char> AddElement(IEnumerable<char> array, int value)
            {
                int counter = 0;
                foreach (char item in array)
                {
                    counter += 1;
                    yield return item;
                }
                while (counter < length)
                {
                    counter += 1;
                    yield return '\0';
                }
                yield break;
            }

            IEnumerable<string> Constructor(int indexator, IEnumerable<string> last, int number)
            {
                if (number != indexator)
                {
                    try
                    {
                        return Constructor(indexator, last.Zip( (GetEnum(indexator, number).Count()>=last.Count() ? GetEnum(indexator, number) : AddElement(GetEnum(indexator, number), last.Count()))
                            , (string first, char second) => (first + Filter(second))), number + 1);
                    }
                    catch
                    {
                        Console.WriteLine("Error");
                        return Constructor(indexator, last.Zip(GetEnum(indexator, number), (string first, char second) => (first + Filter(second))), number + 1);
                    }
                }
                else
                {
                    return last;
                }
            }



            IEnumerable<(string, int, int)> Statistic = (from chars in Construct(length)
                                                        group chars by chars into groups
                                                        orderby groups.Count() descending
                                                        select (groups.Key, groups.Count(), Count())).OrderBy(item=>item.Key.Sum(x=>(int)x)).ToArray();

            foreach ((string, int, int) temp in Statistic)
            {
                Console.WriteLine(temp.Item3 + " - " + temp.Item2 + " - " + temp.Item1);
            }

            Dictionary<(string, int, int), string> haffman = Hafman(Statistic.OrderByDescending(temp => temp.Item2).ToArray(), "");
            Dictionary<(string, int, int), string> shenonfano = ShenonFano(Statistic.OrderByDescending(temp=>temp.Item2).ToArray(), "");
            Dictionary<(string, int, int), string> optimazedshenonfano = shenonfano
                .Select(item => item.Key)
                .Zip((shenonfano.Select(item => item.Value))
                .OrderBy(item => item.Count()), (x, y) =>
                new KeyValuePair<(string, int, int), string>(x, y)).ToDictionary(key => key.Key, value=>value.Value) ;

            /*((string, int, int), string)[] b = a.Select(x => (x.Key,x.Value)).ToArray();

            Console.WriteLine("-----------------------------------------------------------------------------");
            foreach (var c in b.OrderByDescending(l => l.Item1.Item2))
            {
                Console.WriteLine(c.Item1 + " " + c.Item2);
            }
            Console.WriteLine("-----------------------------------------------------------------------------");
            foreach (var c in k)
            {
                Console.WriteLine(c.Key + " " + c.Value);
            }*/


            GetCode =       (from item1 in haffman
                             join item2 in shenonfano on item1.Key.Item1 equals item2.Key.Item1
                             join item3 in optimazedshenonfano on item1.Key.Item1 equals item3.Key.Item1
                             select new KeyValuePair<string, (string, string, string, int)>(item1.Key.Item1, (item1.Value, item2.Value, item3.Value, item1.Key.Item2)))
                             .ToDictionary(key=>key.Key, value=>value.Value);

        }

        Dictionary<(string, int, int), string> ShenonFano(IEnumerable<(string, int, int)> input, string senyorkey)
        {

            (double, double) Summator(IEnumerable<(string, int, int)> value)
            {
                double hj = (value
                            .Sum(i => i.Item2) / 2.0);
                bool flagA = true;
                bool flagB = true;
                IEnumerable<int> su = value
                                    .Select(x => x.Item2).ToArray();

                return (su.Aggregate(0, (x, y) =>
                {
                    if (flagA && (x + y <= hj))
                    {
                        return x + y;
                    }
                    else
                    {
                        flagA = false;
                        return x;
                    }
                }), su.Aggregate(0, (x, y) => {
                    if (flagB && (x <= hj))
                    {
                        return x + y;
                    }
                    else
                    {
                        flagB = false;
                        return x;
                    }
                }));
            }

            IEnumerable<(string, int, int)> TakeWhileAggregate(
                IEnumerable<(string, int, int)> source,
                double seed,
                Func<double, (string, int, int), double> func,
                Func<double, bool> predicate)
            {
                double accumulator = seed;
                foreach ((string, int, int) item in source)
                {
                    accumulator = func(accumulator, item);
                    //Console.WriteLine(accumulator+" "+item.Item2+" "+item.Item1);
                    if (predicate(accumulator))
                    {
                        //Console.WriteLine("-----------------------------------------------------"+item.Item1 + " " + item.Item2);
                        yield return item;
                    }
                    else
                    {
                        yield break;
                    }
                }
                yield break;
            }

            IEnumerable<(string, int, int)> TakeWhileAggregateSpeed
                (
                    IEnumerable<(string, int, int)> source,
                    double seed,
                    Func<double, bool> predicate
                )
            {
                int summ = 0;
                return source.TakeWhile(i => {
                    summ += i.Item2;
                    return predicate(summ);
                });
            }


            switch (input.Count())
            {
                case 0:
                    /*Console.WriteLine("               /0 " + senyorkey);
                    foreach ((string, int, int) x in input)
                    {
                        Console.WriteLine(x.Item1 + " " + x.Item2);
                    }*/
                    return null;
                case 2:
                    /*Console.WriteLine("               /2 " + senyorkey);
                    foreach ((string, int, int) x in input)
                    {
                        Console.WriteLine(x.Item1 + " " + x.Item2);
                    }*/
                    return new Dictionary<(string, int, int), string>()

                    {
                        {input.ElementAt(0), senyorkey+"0"},
                        {input.ElementAt(1), (senyorkey+"1") }
                        };
                case 1:
                    /*Console.WriteLine("               /1 " + senyorkey);
                    foreach ((string, int, int) x in input)
                    {
                        Console.WriteLine(x.Item1 + " " + x.Item2);
                    }*/
                    return new Dictionary<(string, int, int), string>()
                        {{input.ElementAt(0), senyorkey} };
                default:
                    {

                        /*Console.WriteLine("               /3 " + senyorkey, 2);
                        foreach ((string, int, int) x in input)
                        {
                            Console.WriteLine(x.Item1 + " " + x.Item2);
                        }*/

                        //List<int> Item2 = (from temp in input
                        //                         select temp.Item2).ToList();

                        IEnumerable<(string, int, int)> FirstNum;
                        IEnumerable<(string, int, int)> SecondNum;

                        double First, Second;
                        (First, Second) = (Summator(input));


                        double hj = (input.Sum(i => i.Item2) / 2.0);
                        int su = input.Sum(i => i.Item2);
                        if ((hj - First) <= (Second - hj))
                        {
                            //Console.WriteLine("First " + First + " " + Second + " " + ((input.Sum(i => i.Item2) / 2) - First) + " " + (Second - (input.Sum(i => i.Item2) / 2)));
                            FirstNum = TakeWhileAggregate(input, 0, (x, y) => x + y.Item2, (x) => x <= First).ToArray();
                            SecondNum = TakeWhileAggregate(input.Reverse(), 0, (x, y) => x + y.Item2, (x) => x <= (su - First)).Reverse().ToArray();
                            TakeWhileAggregate(input.Reverse(), 0, (x, y) => x + y.Item2, (x) => x <= su + 1).ToArray();
                        }
                        else
                        {
                            //Console.WriteLine("Second "+First+" "+Second+" "+ (hj - First)+" "+ (Second - hj));
                            FirstNum = TakeWhileAggregate(input, 0, (x, y) => x + y.Item2, (x) => x <= Second).ToArray();
                            SecondNum = TakeWhileAggregate(input.Reverse(), 0, (x, y) => x + y.Item2, (x) => x <= (su - Second)).Reverse().ToArray();
                        }



                        try
                        {
                            return ShenonFano(FirstNum, (senyorkey + "0")).Concat(ShenonFano(SecondNum, (senyorkey + "1"))).ToDictionary(x => x.Key, y => y.Value);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());

                            return ShenonFano(FirstNum, (senyorkey + "0")).Concat(ShenonFano(SecondNum, (senyorkey + "1"))).ToDictionary(x => x.Key, y => y.Value);
                        }

                    }
            }
        }

        Dictionary<(string, int, int), string> Hafman(IEnumerable<(string, int, int)> input, string senyorkey)
        {
            IEnumerable<IContainer> ValueNumerable = input.Select(i => new Node(i));
            IEnumerable<IContainer> ValueNumerable2 = ValueNumerable;

            IEnumerable<IContainer> ReduxFunction (ref List<IContainer> date)
            {
                if (date.Count() > 1)
                {
                    IContainer Min1 = date.Last();
                    date.Remove(Min1);
                    IContainer Min2 = date.Last();
                    date.Remove(Min2);
                    Min1.Add("0");
                    Min2.Add("1");
                    IContainer NewNode = new Container((Min1, Min2));
                    date.Add(NewNode);
                    date.Sort();
                    date.Reverse();
                    return ReduxFunction(ref date);
                }
                else
                {
                    return date;
                }
            }
            List<IContainer> k = ValueNumerable.ToList();
            return ReduxFunction(ref k).Min().GetPair;


            

        }

        private string Filter(char c)
        {
            switch (c)
            {
                case ' ': return "|_|";
                case (char)13: return "\\r";
                case (char)10: return "\\n";
                default: return c.ToString();
            }
        }

        int Compare((char, int) x, (char, int) y)
        {
            if (x.Item2 > y.Item2)
            {
                return -1;
            }
            if (x.Item2 < y.Item2)
            {
                return 1;
            }
            return 0;
        }

        public (char, double)[] GetIter
        {
            get
            {
                return _iter;
            }
        }
    }

    interface IContainer: IComparable<IContainer>
    {
        Dictionary<(string, int, int), string> GetPair { get; }
        void Add(string key);
        int GetNume { get; }
    }

    class Container : IContainer
    {
        public Container((IContainer, IContainer) contein)
        {
            _contain = contein;
            
        }


        private (IContainer, IContainer) _contain;

        public int CompareTo(IContainer contein)
        {
            if (contein is null)
            {
                throw new ArgumentException("Некорректное значение параметра Container интерфейса IContainer");
            }
            return GetNume.CompareTo(contein.GetNume);
        }

        public void Add(string key)
        {
            _contain.Item1.Add(key);
            _contain.Item2.Add(key);
        }

        public int GetNume
        {
            get
            {
                return _contain.Item1.GetNume + _contain.Item2.GetNume;
            }
        }

        public Dictionary<(string, int, int), string> GetPair
        {
            get
            {


                    Dictionary<(string, int, int), string> a = _contain.Item1.GetPair;
                    Dictionary<(string, int, int), string> b = _contain.Item2.GetPair;
                    var c = a.Concat(b);
                try
                { 
                    return c.ToDictionary(x => x.Key, y => y.Value);
                }
                catch
                {
                    //throw new Exception();
                    return a;
                }
            }
        }
    }




    class Node : IContainer
    {
        public Node((string, int, int) value)
        {
            _value = value;
            _key = "";
        }

        private string _key;

        private (string, int, int) _value;

        public int CompareTo(IContainer contein)
        {
            if (contein is null)
            {
                throw new ArgumentException("Некорректное значение параметра Node интерфейса IContainer");
            }
            return GetNume.CompareTo(contein.GetNume);
        }

        public void Add(string key)
        {
            _key = key + _key;
        }

        public int GetNume 
        {
            get
            {
                return _value.Item2;
            }
        }

        public Dictionary<(string, int, int), string> GetPair
        {
            get
            {
                return new Dictionary<(string, int, int), string> { { _value, _key } };
            }
        }
    }

}    

