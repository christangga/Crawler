using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler {
	class Program {
		static void Main(string[] args) {
			//Crawler crawler = new Crawler("http://informatika.stei.itb.ac.id/~rinaldi.munir/", 0);
			Crawler crawler = new Crawler("http://www.itb.ac.id/", 0);
			Console.Read();
		}
	}
}
