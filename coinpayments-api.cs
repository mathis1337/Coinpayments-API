private string s_privkey = "privateKey";
private string s_pubkey = "publicKey";
private static readonly Encoding encoding = Encoding.UTF8;

void Main()
{
	CoinBalances();
	//GetTransactionInfo();
}

public void CreateTransaction()
{
	SortedList<string, string> parms = new SortedList<string, string>();

	parms.Add("amount", "0.10");
	parms.Add("currency1", "USD");
	parms.Add("currency2", "ETH");
	
	dynamic objValue;
	
	var ret = CallAPI("create_transaction", parms);

	ret.Dump();
	Dictionary<string, object> dict = new Dictionary<string, object>();
//
//	//Get Value to pull dictionary items out.
	ret.TryGetValue("result", out objValue);
	dict = objValue;
	
	dict["address"].Dump();
}

public void CoinBalances()
{
	SortedList<string, string> parms = new SortedList<string, string>();
	//parms.Add("amount", "0.10");
	

	dynamic objValue;

	var ret = CallAPI("balances", parms);

	Dictionary<string, object> dict = new Dictionary<string, object>();
	Dictionary<string, object> btc = new Dictionary<string, object>();
	Dictionary<string, object> eth = new Dictionary<string, object>();
	//
	//
	//	//Get Value to pull dictionary items out.
	ret.TryGetValue("result", out objValue);
	dict = objValue;

	btc = (Dictionary<string, object>)dict["BTC"].Dump();
	eth = (Dictionary<string, object>)dict["ETH"].Dump();
	
	btc["balancef"].ToString().Dump();
	eth["balancef"].ToString().Dump();

	
}

//Get tran method. Will need to pass in txid
public void GetTransactionInfo()
{
	SortedList<string, string> parms = new SortedList<string, string>();
	
	parms.Add("txid", "CPBI1EVMIABO4B4U4YUBTL4R90");

	var ret = CallAPI("get_tx_info", parms).Dump();
	dynamic objValue;
	string statusTest;
	int status = 0;
	Dictionary<string, object> dict = new Dictionary<string, object>();
	
	//Get Value to pull dictionary items out.
	ret.TryGetValue("result", out objValue);
	dict = objValue;
	dict.Dump();
	dict["status_text"].Dump();
	dict["status"].Dump();
	dict["coin"].Dump();
	dict["amountf"].Dump();

}

public void CoinPayments()
{
	if (s_privkey.Length == 0 || s_pubkey.Length == 0)
	{
		throw new ArgumentException("Private or Public Key is empty");
	}
}

public void GetExchangeRates()
{
	SortedList<string, string> parms = new SortedList<string, string>();

	parms.Add("accepted", "1");


	var ret = CallAPI("rates", parms);
	ret.Dump();

}

public void GetDepositAddress()
{
	SortedList<string, string> parms = new SortedList<string, string>();

	parms.Add("currency", "ETH");


	var ret = CallAPI("get_deposit_address", parms);
	ret.Dump();

}

public Dictionary<string, dynamic> CallAPI(string cmd, SortedList<string, string> parms = null)
{
	if (parms == null)
	{
		parms = new SortedList<string, string>();
	}
	parms["version"] = "1";
	parms["key"] = s_pubkey;
	parms["cmd"] = cmd;

	string post_data = "";
	foreach (KeyValuePair<string, string> parm in parms)
	{
		if (post_data.Length > 0) { post_data += "&"; }
		post_data += parm.Key + "=" + Uri.EscapeDataString(parm.Value);
	}

	byte[] keyBytes = encoding.GetBytes(s_privkey);
	byte[] postBytes = encoding.GetBytes(post_data);
	var hmacsha512 = new System.Security.Cryptography.HMACSHA512(keyBytes);
	string hmac = BitConverter.ToString(hmacsha512.ComputeHash(postBytes)).Replace("-", string.Empty);

	// do the post:
	System.Net.WebClient cl = new System.Net.WebClient();
	cl.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
	cl.Headers.Add("HMAC", hmac);
	cl.Encoding = encoding;

	var ret = new Dictionary<string, dynamic>();
	try
	{
		string resp = cl.UploadString("https://www.coinpayments.net/api.php", post_data);
		var decoder = new System.Web.Script.Serialization.JavaScriptSerializer();
		ret = decoder.Deserialize<Dictionary<string, dynamic>>(resp);
	}
	catch (System.Net.WebException e)
	{
		ret["error"] = "Exception while contacting CoinPayments.net: " + e.Message;
	}
	catch (Exception e)
	{
		ret["error"] = "Unknown exception: " + e.Message;
	}

	return ret;
}
// Define other methods and classes here
