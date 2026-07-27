using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;

class Program {
	static async Task Main() {
		Buff buff = new();
		await buff.SetTarget(-1);
		await buff.GetProxies();
		await buff.TestProxies();
		await Task.Delay(2000);
		int round = 0;
		while (true) {
			await buff.RunProxies(round++);
			round %= 6;
		}
	}
}
public class Buff {
	private static string[] ProxiesSources = [
		"https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/http.txt",
		"https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/socks4.txt",
		"https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/socks5.txt",
		"https://raw.githubusercontent.com/jetkai/proxy-list/main/online-proxies/txt/proxies.txt",
		"https://raw.githubusercontent.com/monosans/proxy-list/main/proxies/all.txt", //Linus Torvalds
		"https://raw.githubusercontent.com/roosterkid/openproxylist/main/HTTPS_RAW.txt",
		"https://raw.githubusercontent.com/almroot/proxylist/master/list.txt",
		"https://raw.githubusercontent.com/ShiftyTR/Proxy-List/master/proxy.txt",
		"https://raw.githubusercontent.com/hookzof/socks5_list/master/proxy.txt",
		"https://raw.githubusercontent.com/clarketm/proxy-list/master/proxy-list-raw.txt",
		"https://raw.githubusercontent.com/proxifly/free-proxy-list/main/proxies/all/data.txt",
		"https://raw.githubusercontent.com/ALIILAPRO/Proxy/main/http.txt",
		"https://raw.githubusercontent.com/ALIILAPRO/Proxy/main/socks4.txt",
		"https://raw.githubusercontent.com/ALIILAPRO/Proxy/main/socks5.txt",
		"https://raw.githubusercontent.com/Zaeem20/FREE_PROXIES_LIST/master/http.txt",
		"https://raw.githubusercontent.com/Zaeem20/FREE_PROXIES_LIST/master/https.txt",
		"https://raw.githubusercontent.com/Zaeem20/FREE_PROXIES_LIST/master/socks4.txt",
		"https://raw.githubusercontent.com/Zaeem20/FREE_PROXIES_LIST/master/socks5.txt",
		"https://raw.githubusercontent.com/vakhov/fresh-proxy-list/master/proxylist.txt",
		"https://raw.githubusercontent.com/r00tee/Proxy-List/main/Https.txt",
		"https://raw.githubusercontent.com/r00tee/Proxy-List/main/Socks4.txt",
		"https://raw.githubusercontent.com/r00tee/Proxy-List/main/Socks5.txt",
		"https://github.com/databay-labs/free-proxy-list/raw/master/http.txt",
		"https://github.com/databay-labs/free-proxy-list/raw/master/socks4.txt",
		"https://github.com/databay-labs/free-proxy-list/raw/master/socks5.txt",
		"https://github.com/elliottophellia/proxylist/raw/master/results/mix_checked.txt",
		"https://github.com/rdavydov/proxy-list/raw/main/proxies/http.txt",
		"https://github.com/rdavydov/proxy-list/raw/main/proxies/socks4.txt",
		"https://github.com/rdavydov/proxy-list/raw/main/proxies/socks5.txt",
		"https://github.com/prxchk/proxy-list/raw/main/all.txt",
		"https://github.com/iplocate/free-proxy-list/raw/refs/heads/main/all-proxies.txt",
		"https://api.proxyscrape.com/v2/?request=displayproxies&protocol=all&timeout=10000&country=all&simplified=true",
	];
	private static HttpClient UnproxiedClient = new();
	private static List<HttpClient>? ProxiedClients;
	private static List<bool>? AdvancedProxies;
	private static List<string>? ProxiesToUse;
	private static StringContent? Payload;
	private static string Target = "https://api.scratch.mit.edu/users/thanh_cundz/projects/1334396955/views";
	private async Task<string[]> GetProxiesFromSource(string ProxiesSource) {
		try {
			string content = await UnproxiedClient.GetStringAsync(ProxiesSource);
			string[] proxies = content
				.Replace("http://", "")
				.Replace("socks4://", "")
				.Replace("socks5://", "")
				.Replace("https://", "")
				.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
			Console.WriteLine($"Fetched {proxies.Length} proxies from {ProxiesSource}");
			return proxies;
		} catch {
			Console.WriteLine($"Failed to fetch {ProxiesSource}");
			return new string[0];
		}
	}
	public async Task GetProxies() {
		int sourcesCount = ProxiesSources.Length;
		Task<string[]>[] workers = new Task<string[]>[sourcesCount];
		for (int i = 0; i < sourcesCount; i++) {
			workers[i] = GetProxiesFromSource(ProxiesSources[i]);
		}
		HashSet<string> result = new();
		foreach (string[] proxies in await Task.WhenAll(workers)) {
			foreach (string proxy in proxies)
				result.Add(proxy);
		}
		ProxiesToUse = result.ToList();
	}
	public async Task TestProxies() {
		ProxiedClients = new List<HttpClient>();
		HttpClientHandler handler;
		Payload = new StringContent("{}", Encoding.UTF8, "application/json");
		foreach (string proxy in ProxiesToUse!) {
			handler = new HttpClientHandler
			{
				Proxy = new WebProxy(proxy),
				UseProxy = true
			};
			HttpClient ProxiedClient = new HttpClient(handler);
			ProxiedClient.Timeout = TimeSpan.FromSeconds(10);
			ProxiedClients.Add(ProxiedClient);
		}
		int clientsCount = ProxiedClients.Count;
		if (clientsCount == 0) throw new Exception("No proxy found!");
		HashSet<int> liveSet = new();
		int[] proxiesInfo = new int[clientsCount];
		for (int j = 0; j < 3; j++) {
			Task<int[]>[] workers = new Task<int[]>[clientsCount];
			for (int i = 0; i < clientsCount; i++) {
				workers[i] = TestProxyWorker(i);
			}
			foreach (int[] status in await Task.WhenAll(workers)) {
				if (status[0] >= 0)
					liveSet.Add(status[0]);
				if (status[1] == 1)
					proxiesInfo[status[0]]++;
			}
		}
		AdvancedProxies = proxiesInfo
			.Select(x => x == 3)
			.ToList();
		for (int i = clientsCount-1; i >= 0; i--) {
			if (!liveSet.Contains(i)) {
				ProxiedClients?[i].Dispose();
				ProxiedClients?.RemoveAt(i);
				AdvancedProxies?.RemoveAt(i);
			}
		}
		Console.WriteLine($"total {ProxiedClients?.Count} live proxies");
		ProxiesToUse = null;
	}
	private async Task<int[]> TestProxyWorker(int id) {
		HttpClient client = ProxiedClients?[id]!;
		try {
			var res = await client?.PostAsync(Target, Payload)!;
			int statusCode = (int)res.StatusCode;
			if (statusCode == 200 || statusCode == 429) {
				Console.WriteLine($"works: {id}");
			}
			return new int[] {id, statusCode == 200 ? 1 : 0};
		} catch {
			return new int[] {-1, 0};
		}
	}
	public async Task RunProxies(int round) {
		int clientsCount = (int)ProxiedClients?.Count!;
		if (clientsCount == 0) throw new Exception("No live proxy found!");
		Task[] workers = new Task[clientsCount];
		bool cancel = false;
		for (int i = 0; i < clientsCount; i++) {
			cancel = !(round == 0 || AdvancedProxies![i]);
			workers[i] = RunProxyWorker(i, cancel);
		}
		await Task.WhenAll(workers);
	}
	private async Task RunProxyWorker(int id, bool cancel) {
		if (cancel) return;
		HttpClient client = ProxiedClients?[id]!;
		try {
			var res = await client?.PostAsync(Target, Payload)!;
			int statusCode = (int)res.StatusCode;
			Console.WriteLine($"received: {statusCode} {id}");
		} catch {}
	}
	public async Task SetTarget(int id) {
		int trueId = id;
		if (trueId == -1) {
			Console.Write("Enter project ID: ");
			if (!int.TryParse(Console.ReadLine(), out trueId)) {
				trueId = -1;
			}
		}
		if (trueId == -2) {
			Target = "https://api.scratch.mit.edu/users/thanh_cundz/projects/1334396955/views";
			return;
		}
		JsonDocument projectData = JsonDocument.Parse(
			await UnproxiedClient.GetStringAsync($"https://api.scratch.mit.edu/projects/{trueId}")
		);
		JsonElement author = projectData.RootElement.GetProperty("author");
		string projectAuthorUsername = author.GetProperty("username").GetString()!;
		Target = $"https://api.scratch.mit.edu/users/{projectAuthorUsername}/projects/{trueId}/views";
		Console.WriteLine(Target);
	}
}