dynamodb-geo-csharp
===================

C# Port of the AWS DynamoDB GeoSpatial library

The Geo Library for [Amazon DynamoDB][dynamodb] enables .NET developers to easily 
create and query geospatial data. The library takes care of managing the geohash indexes 
required for fast and efficient execution of location-based queries over a 
table of items representing points of interest - latitude/longitude pairs.

Along with this library there's a sample application demonstrating usage of the library 
for a cloud-backed mobile app development scenario. You can get up and
running quickly with a local IIS Express application and running the unit tests.

##Features
* **Box Queries:** Return all of the items that fall within a pair of geo points that define a rectangle as projected onto a sphere.
* **Radius Queries:** Return all of the items that are within a given radius of a geo point.
* **Basic CRUD Operations:** Create, retrieve, update, and delete geospatial data items.
* **Easy Integration:** Adds functionality to the AWS SDK for .NET in your server application.
* **Customizable:** Access to raw request and result objects from the AWS SDK for .NET.

##Getting Started
###Setup Environment
1. **Sign up for AWS** - Before you begin, you need an AWS account. Please see the [AWS Account and Credentials][docs-signup] section of the developer guide for information about how to create an AWS account and retrieve your AWS credentials.
2. **Minimum .NET requirements** - To run the SDK you will need **.NET 4.5+**. 
3. **Download Geo Library for Amazon DynamoDB** - To download the code from GitHub, simply clone the repository by typing: `git clone https://github.com/onovotny/dynamodb-geo-csharp.git`.
4. **NuGet Package** - For use in your applicatin, it's recommended to use the Nuget Package. Run `Install-Package DynamoDB.Geo` in the package manager console.

##Limitations

###No composite key support
Currently, the library does not support composite keys. You may want to add tags such as restaurant, bar, and coffee shop, and search locations of a specific category; however, it is currently not possible. You need to create a table for each tag and store the items separately.

###Queries retrieve all paginated data
Although low level [DynamoDB Query][dynamodb-query] requests return paginated results, this library automatically pages through the entire result set. When querying a large area with many points, a lot of Read Capacity Units may be consumed.

###More Read Capacity Units
The library retrieves candidate Geo points from the cells that intersect the requested bounds. The library then post-processes the candidate data, filtering out the specific points that are outside the requested bounds. Therefore, the consumed Read Capacity Units will be higher than the final results dataset.

###High memory consumption
Because all paginated `Query` results are loaded into memory and processed, it may consume substantial amounts of memory for large datasets.

###The server is essential
Because Geo Library calls multiple DynamoDB `Query` requests and processes the results in memory, it is not suitable for mobile device use. You should maintain a .NET server/Website, and use the library on the server.

###Dataset density limitation
The Geohash used in this library is roughly centimeter precision. Therefore, the library is not suitable if your dataset has much higher density.

##Reference

###Amazon DynamoDB
* [Amazon DynamoDB][dynamodb]
* [Amazon DynamoDB Forum][dynamodb-forum]


[dynamodb]: http://aws.amazon.com/dynamodb
[dynamodb-query]: http://docs.aws.amazon.com/amazondynamodb/latest/APIReference/API_Query.html
[dynamodb-forum]: https://forums.aws.amazon.com/forum.jspa?forumID=131
