using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Xml.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using UnityEditor.PackageManager;
using Debug = UnityEngine.Debug;

public class MongoDBTool
{
    protected MongoClient client;

    //const string connectionString = "mongodb://localhost";
    //const string connectionString = "mongodb+srv://<username>:<password>@<cluster-address>/test?w=majority";
    const string connectionString = "mongodb://192.168.1.106:27017/?readPreference=primary&directConnection=true&ssl=false";

    public MongoDBTool()
    {
        client = new MongoClient(connectionString);
        Debug.Log($"connect to: {connectionString}");
    }

    public void Connect() { }
    private void Dispose()
    {
        client = null;
    }

    // 新建表和字段
    public void CreateDatabase()
    {
        var db = client.GetDatabase("Student");
        db.CreateCollection("Id");
        db.CreateCollection("Name");
        db.CreateCollection("Grade");
        db.CreateCollection("Class");
        db.CreateCollection("DateTime");
        db.CreateCollection("PA");
    }
    // 删除表
    public void DropDatabase()
    {
        var result = client.DropDatabaseAsync("Student");
        Debug.Log(result.ToJson());
    }
    // 删除字段
    public void DropCollection()
    {
        var db = client.GetDatabase("Student");
        db.DropCollection("PA");
    }

    public void QueryDatabase()
    {
        var search1 = client.ListDatabaseNames().ToList();
        Debug.Log($"有{search1.Count}个database");
        for (int i = 0; i < search1.Count; i++)
        {
            Debug.Log($"[{i}]---{search1[i]}");
        }
        //有4个database
        //[0]-- - admin
        //[1]-- - config
        //[2]-- - local
        //[3]-- - stickerDB / Student
    }

    void Test()
    {
        var connectionString = Environment.GetEnvironmentVariable("MONGODB_URI");
        if (connectionString == null)
        {
            Debug.Log("You must set your 'MONGODB_URI' environment variable. To learn how to set it, see https://www.mongodb.com/docs/drivers/csharp/current/quick-start/#set-your-connection-string");
            Environment.Exit(0);
        }
        var client = new MongoClient(connectionString);
        var collection = client.GetDatabase("sample_mflix").GetCollection<BsonDocument>("movies");
        var filter = Builders<BsonDocument>.Filter.Eq("title", "Back to the Future");
        var document = collection.Find(filter).First();
        Debug.Log(document);
    }



    public void Insert()
    {

    }
    public void Update() { }
    public void Query()
    {
        //  根据数据库名称实例化数据库
        var database = client.GetDatabase("stickerDB");

        // 根据集合名称获取集合
        var collection = database.GetCollection<BsonDocument>("Users");
        var filter = new BsonDocument();

        // 查询集合中的文档
        var search2 = Task.Run(async () => await collection.Find(filter).ToListAsync()).Result;
        // 循环遍历输出
        search2.ForEach(p =>
        {
            Debug.Log($"姓名：{p["name"]}，球衣号码：{p["number"]}");
        });

        //$lt    <   (less  than)
        //$lte   <=  (less than  or equal to)
        //$gt    >   (greater  than)
        //$gte   >=  (greater  than or equal to)
        //var filter3 = Builders<BsonDocument>.Filter.Eq("number", 10); //等于
        var filter3 = Builders<BsonDocument>.Filter.Gte("number", 10); //大于等于
        var result = collection.Find(filter3);
        Debug.Log($"result={result.CountDocuments()}");
    }
}