using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QueryBuilder;

namespace QueryBuilder {
	class Program {
		static void Main(string[] args) {
			Searcher searcher = new Searcher();
			searcher.search();
			Console.Read();
		}
	}
}
