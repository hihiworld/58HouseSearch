using Dapper;
using HouseMap.Common;
using HouseMap.Dao.DBEntity;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace HouseMap.Dao
{
    public class NewHouseDapper : NewBaseDapper
    {
        public NewHouseDapper(IOptions<AppSettings> options) : base(options)
        {
        }

        public void BulkInsertHouses(List<DBHouse> houses)
        {
            if (houses == null || houses.Count == 0)
            {
                return;
            }
            var tableName = SourceTool.GetHouseTableNameDic()[houses.FirstOrDefault().Source];
            using (IDbConnection dbConnection = GetConnection())
            {
                dbConnection.Open();
                IDbTransaction transaction = dbConnection.BeginTransaction();
                var result = dbConnection.Execute("INSERT INTO " + tableName + @" 
                                     (`Title`, `Text`,  
                                    `PicURLs`, `Location`, 
                                    `City`,
                                     `Longitude`, `Latitude`,
                                    `RentType`,`Tags`, 
                                    `PubTime`, `OnlineURL`,
                                     `Price`,`Labels`,
                                    `Source`,`Id`) 
                                     VALUES (@Title, @Text,
                                            @PicURLs, @Location,
                                            @City, 
                                            @Longitude,@Latitude,
                                            @RentType,@Tags,
                                            @PubTime,@OnlineURL,
                                            @Price,@Labels,
                                            @Source,@Id)  ON DUPLICATE KEY UPDATE UpdateTime=now();",
                                     houses, transaction: transaction);
                result = dbConnection.Execute(@"INSERT INTO HouseData 
                        (`JsonData`,`Id`,`OnlineURL`) 
                        VALUES (@JsonData,@Id,@OnlineURL) ON DUPLICATE KEY UPDATE UpdateTime=now();",
                        houses, transaction: transaction);
                transaction.Commit();
            }

        }



        public List<DBHouse> SearchHouses(NewHouseCondition condition)
        {
            var houses = new List<DBHouse>();
            using (IDbConnection dbConnection = GetConnection())
            {
                dbConnection.Open();
                houses = dbConnection.Query<DBHouse>(condition.QueryText, condition).ToList();
                return houses;
            }
        }

        public DBHouse FindById(string houseId)
        {
            using (IDbConnection dbConnection = GetConnection())
            {
                dbConnection.Open();
                foreach (var tableName in SourceTool.GetHouseTableNameDic().Values)
                {
                    var house = dbConnection.Query<DBHouse>(@"SELECT Id,
                                            OnlineURL,
                                            Title,
                                            Location,
                                            Price,
                                            PubTime,
                                            City,
                                            Source,
                                            PicURLs,
                                            Labels,
                                            Tags,
                                            RentType,
                                            Latitude,
                                            Longitude,
                                            Text "
                                            + $" from { tableName } where id = @HouseId", new { HouseId = houseId })
                                .FirstOrDefault();
                    if (house != null)
                    {
                        return house;
                    }
                }
                return null;
            }
        }
    }
}