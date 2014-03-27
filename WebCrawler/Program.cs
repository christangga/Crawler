using System;
using System.IO;
using System.Collections.Generic;

namespace WebCrawler {
	class Program {
		static void Main(string[] args) {
			StreamReader setting = new StreamReader("setting.txt");
			Crawler crawler = new Crawler();
			
			Console.WriteLine("1. BFS");
			Console.WriteLine("2. DFS");
			int crawltype = (int.Parse(setting.ReadLine())) - 1;
			Console.WriteLine("1. Depth Limit");
			Console.WriteLine("2. Pages Limit");
			int limitType = int.Parse(setting.ReadLine());
			Console.WriteLine("Max Pages/Depth:");
			int limit = int.Parse(setting.ReadLine());
			if(limitType == 1)
				crawler.setMaxDepth(limit);
			else if(limitType == 2)
				crawler.setMaxPage(limit);
			Console.WriteLine("Enter link:");
			String link = setting.ReadLine();
			crawler.crawl(link, crawltype, limitType);
			//Crawler crawler = new Crawler("http://informatika.stei.itb.ac.id/~rinaldi.munir/", crawltype, 0);
			//Crawler crawler = new Crawler("http://www.wikipedia.org", crawltype, 0);
			//Crawler crawler = new Crawler("http://www.facebook.com", crawltype, 0);
			//Crawler crawler = new Crawler("http://rinaldimunir.wordpress.com/", crawltype, 0);
			setting.Close();
			Console.Read();
		}
	}
}
