# Sigin.ObjectId

The `ObjectId` class is a C# implementation of the ObjectId data type used in MongoDB databases. An ObjectId is a 12-byte unique identifier that consists of a timestamp, machine identifier, process ID, and increment. It is commonly used as a primary key in MongoDB collections and is guaranteed to be unique within a single MongoDB instance.

## Installation

TODO

## Usage

To use the `ObjectId` class, simply call the static `NewObjectId()` method to generate a new unique identifier.

Here's an example of how to generate a new ObjectId:

```c#
using Sigin.ObjectId;

ObjectId objectId = ObjectId.NewObjectId();
```

You can also convert an ObjectId to a string using the ToString() method, and convert a string to an ObjectId using the static Parse() or TryParse() methods. For example:

```c#
ObjectId objectId = ObjectId.NewObjectId();
string objectIdString = objectId.ToString();

ObjectId parsedObjectId = ObjectId.Parse(objectIdString);
```

## ObjectID format

The [ObjectID](https://www.mongodb.com/docs/manual/reference/method/ObjectId/) BSON type is a 12-byte value consisting of three different portions (fields):

* a 4-byte big endian field represents the seconds since the Unix epoch (Jan 1st, 1970, midnight UTC). It is an ever increasing value that will have a range until about Jan 7th, 2106,
* a 5-byte field consisting of a random value generated once per process. This random value is unique to the machine and process,
* a 3-byte big endian counter, initialised to a random value, increased by 1 for every ObjectID creation.

```text
  4 byte timestamp    5 byte process unique   3 byte counter
|<----------------->|<---------------------->|<------------>|
[----|----|----|----|----|----|----|----|----|----|----|----]
0                   4                   8                   12
```

## Benchmarks

TODO

## Contributions

I encourage contributions to Sigin.ObjectId! If you come across a bug or have a suggestion for a new feature, don't hesitate to open an issue or submit a pull request on GitHub. I'm constantly seeking ways to enhance the library and provide more value to the community.

## Credits

It's worth mentioning that the implementation of the `ObjectId` class in this project was highly inspired by the work of [vanbukin](https://github.com/vanbukin) on the [dodopizza/primitives](https://github.com/dodopizza/primitives) project. It was used as an example and adapted.
