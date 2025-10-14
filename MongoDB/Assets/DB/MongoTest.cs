using UnityEngine;

public class MongoTest : MonoBehaviour
{
    void Start()
    {
        var mongo = new MongoDBTool();

        mongo.CreateDatabase();
        //mongo.DropDatabase();
        mongo.QueryDatabase();

        //mongo.CreateCollection();
        //mongo.DropCollection();
    }
}
