using Example.Console.EF;
using System.Linq;
using IQueryableExtensions;

namespace Example.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var start = 1;              
                var pageSize = 25;

                // before
                //for (int i = 1; i < 10; i++)
                //{
                //    int[] values = Enumerable.Range(start, pageSize).ToArray();

                //    var result = db.Phones
                //        .Where(x => values.Contains(x.Id))
                //        .ToList();

                //    start = pageSize * i;
                //}

                // after
                for (int i = 1; i < 10; i++)
                {
                    int[] values = Enumerable.Range(start, pageSize).ToArray();

                    var result = db.Phones
                        .In(values, d => d.Id)
                        .ToList();

                    start = pageSize * i;
                }
            }

 
            System.Console.WriteLine("Hello");
        }
    }
}
