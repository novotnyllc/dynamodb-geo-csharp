using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Amazon.Geo.Util
{
    public class GeoTableUtil
    {
        public static CreateTableRequest GetCreateTableRequest(GeoDataManagerConfiguration config)
        {
            var req = new CreateTableRequest
            {
                TableName = config.TableName,
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 10,
                    WriteCapacityUnits = 5
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        KeyType = KeyType.HASH,
                        AttributeName = config.HashKeyAttributeName
                    },
                    new KeySchemaElement
                    {
                        KeyType = KeyType.RANGE,
                        AttributeName = config.RangeKeyAttributeName
                    }
                },
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeType = ScalarAttributeType.N,
                        AttributeName = config.HashKeyAttributeName
                    },
                    new AttributeDefinition
                    {
                        AttributeType = ScalarAttributeType.S,
                        AttributeName = config.RangeKeyAttributeName,
                    },
                    new AttributeDefinition
                    {
                        AttributeType = ScalarAttributeType.N,
                        AttributeName = config.GeohashAttributeName
                    }
                },
                LocalSecondaryIndexes = new List<LocalSecondaryIndex>
                {
                    new LocalSecondaryIndex
                    {
                        IndexName = config.GeohashIndexName,
                        KeySchema = new List<KeySchemaElement>
                        {
                            new KeySchemaElement
                            {
                                KeyType = KeyType.HASH,
                                AttributeName = config.HashKeyAttributeName
                            },
                            new KeySchemaElement
                            {
                                KeyType = KeyType.RANGE,
                                AttributeName = config.RangeKeyAttributeName
                            }
                        },
                        Projection = new Projection
                        {
                            ProjectionType = ProjectionType.ALL
                        }

                    }
                }
            };

            return req;
        }
    }
}
