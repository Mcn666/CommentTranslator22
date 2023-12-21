using Newtonsoft.Json;

var word = Console.ReadLine();
var words = Dictionary.ParseString.GetWordArray(word);

var beginTime = DateTime.Now;

foreach (var item in words)
{
    var res = Dictionary.Dictionary.Query(item);
    if (res != null)
    {
        Console.WriteLine(res.en);
        Console.WriteLine(res.zh);
    }
    var endTime = DateTime.Now;
    var oTime = endTime.Subtract(beginTime);
    Console.WriteLine(oTime.TotalSeconds);
}
