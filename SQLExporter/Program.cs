using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLExporter {
	class Program {
		static void Main(string[] args) {
			Exporter exporter = new Exporter();
			exporter.export();
			Console.Read();
		}
	}
}
