using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InsireDB
{
	class Program
	{
		static void Main(string[] args)
		{
			//SimpleIRCClient s = new SimpleIRCClient();
			CounterList list = new CounterList();

			Counter cntr = new Counter("test");
			list.Add("!test", cntr);

			if (list.Increase(cntr)) Console.WriteLine("Success!");
			Thread.Sleep(10001);
			if (list.Increase(cntr)) Console.WriteLine("Success!");
			Console.WriteLine("done!");
			Console.ReadKey();
		}
	}
}
