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
		while (true) {
			await buff.RunProxies();
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
			Console.WriteLine($"Lấy được {proxies.Length} proxy từ nguồn {ProxiesSource}");
			return proxies;
		} catch {
			Console.WriteLine($"Không thể lấy proxy từ nguồn {ProxiesSource}");
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
		if (clientsCount == 0) {
			Console.WriteLine("[-] Lỗi: Không có proxy nào được tìm thấy! Hãy kiểm tra lại kết nối của bạn.");
			Console.ReadLine(); // Cho người dùng biết
			Environment.Exit(0);
		}
		HashSet<int> liveSet = new();
		for (int j = 0; j < 3; j++) {
			Task<int>[] workers = new Task<int>[clientsCount];
			for (int i = 0; i < clientsCount; i++) {
				workers[i] = TestProxyWorker(i);
			}
			foreach (int id in await Task.WhenAll(workers)) {
				if (id >= 0)
					liveSet.Add(id);
			}
		}
		for (int i = clientsCount-1; i >= 0; i--) {
			if (!liveSet.Contains(i)) {
				ProxiedClients?[i].Dispose();
				ProxiedClients?.RemoveAt(i);
			}
		}
		Console.WriteLine($"Tìm thấy tổng cộng {ProxiedClients?.Count} proxy hoạt động");
		ProxiesToUse = null;
	}
	private async Task<int> TestProxyWorker(int id) {
		HttpClient client = ProxiedClients?[id]!;
		try {
			var res = await client?.PostAsync(Target, Payload)!;
			int statusCode = (int)res.StatusCode;
			if (statusCode == 200 || statusCode == 429) {
				Console.WriteLine($"Tìm thấy proxy hoạt động: {id}");
			}
			return id;
		} catch {
			return -1;
		}
	}
	public async Task RunProxies() {
		int clientsCount = (int)ProxiedClients?.Count!;
		if (clientsCount == 0) {
			Console.WriteLine("[-] Lỗi: Không có proxy hoạt động nào được tìm thấy!");
			Console.ReadLine(); // Cho người dùng biết
			Environment.Exit(0);
		}
		Task[] workers = new Task[clientsCount];
		for (int i = 0; i < clientsCount; i++) {
			workers[i] = RunProxyWorker(i);
		}
		await Task.WhenAll(workers);
	}
	private async Task RunProxyWorker(int id) {
		HttpClient client = ProxiedClients?[id]!;
		try {
			var res = await client?.PostAsync(Target, Payload)!;
			int statusCode = (int)res.StatusCode;
			Console.WriteLine($"Đã nhận phản hồi từ proxy: {statusCode} {id}");
		} catch {}
	}
	public async Task SetTarget(int id) {
    int trueId = id;
    
    // 1. LỚP BẢO VỆ ĐẦU VÀO: Ép người dùng nhập đúng số mới cho đi tiếp
    if (trueId == -1) {
        while (true) {
            Console.Write("Nhập ID dự án (VD: 12345678): ");
            string input = Console.ReadLine() ?? "";

			// Dành cho lối tắt -2 của tác giả (nếu bạn vẫn muốn giữ), đảo lên đầu
            if (input == "-2") {
                trueId = -2;
                break;
            }
			
            // Kiểm tra xem input có phải là số và phải lớn hơn 0
            if (int.TryParse(input, out trueId) && trueId > 0) {
                break; // Nhập đúng số hợp lệ -> Thoát vòng lặp để chạy tiếp
            
            
            // Nếu nhập sai, báo lỗi và vòng lặp sẽ bắt nhập lại
            Console.WriteLine("[-] Lỗi: ID dự án không hợp lệ. Vui lòng chỉ nhập các con số (VD: 12345678)!");
        }
    }

    if (trueId == -2) {
        Target = "https://api.scratch.mit.edu/users/thanh_cundz/projects/1334396955/views";
        Console.WriteLine($"[+] Target: {Target}");
        return;
    }

    // 2. LỚP BẢO VỆ API: Dùng Try/Catch để tránh văng app khi không tìm thấy project
    try {
        string jsonResponse = await UnproxiedClient.GetStringAsync($"https://api.scratch.mit.edu/projects/{trueId}");
        JsonDocument projectData = JsonDocument.Parse(jsonResponse);
        
        JsonElement author = projectData.RootElement.GetProperty("author");
        string projectAuthorUsername = author.GetProperty("username").GetString()!;
        
        Target = $"https://api.scratch.mit.edu/users/{projectAuthorUsername}/projects/{trueId}/views";
        Console.WriteLine($"[+] Target: {Target}");
    }
    catch (HttpRequestException) {
        // Lỗi này xảy ra khi máy chủ Scratch trả về 404 (ID không tồn tại) hoặc mất mạng
        Console.WriteLine("\n[-] LỖI NGHIÊM TRỌNG: Không tìm thấy Project này trên Scratch hoặc mất mạng internet.");
        Console.WriteLine("Chương trình sẽ dừng lại. Vui lòng mở lại và nhập đúng ID!");
		Console.ReadLine(); // Cho người dùng biết
        Environment.Exit(0); // Dừng chương trình một cách êm ái thay vì Crash tung tóe
    }
    catch (Exception ex) {
        // Bắt tất cả các lỗi không lường trước được (Ví dụ lỗi giải mã JSON)
        Console.WriteLine($"\n[-] Lỗi không xác định: {ex.Message}");
		Console.ReadLine(); // Cho người dùng biết
        Environment.Exit(0);
    }
}
