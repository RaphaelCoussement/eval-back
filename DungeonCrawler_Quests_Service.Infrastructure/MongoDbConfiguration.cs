using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using Rebus.Sagas;

namespace DungeonCrawler_Quests_Service.Infrastructure;

/// <summary>
/// Configuration globale de MongoDB pour la sérialisation
/// Ca permet d'eviter des erreurs avec Rebus/RabbitMQ et les GUID
/// </summary>
public static class MongoDbConfiguration
{
    private static bool _initialized;
    private static readonly object Lock = new();

    /// <summary>
    /// Configure le sérialiseur BSON pour MongoDB
    /// </summary>
    public static void Configure()
    {
        lock (Lock)
        {
            if (_initialized) return;

            // Configure la représentation des Guid en Standard pour éviter l'erreur "Unspecified"
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            // Convention camelCase SAUF pour les types ISagaData (pour Rebus)
            var conventionPack = new ConventionPack
            {
                new ConditionalCamelCaseConvention(),
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String)
            };

            ConventionRegistry.Register("DungeonCrawlerConventions", conventionPack, _ => true);

            _initialized = true;
        }
    }

    /// <summary>
    /// Convention qui applique camelCase sauf pour les types ISagaData
    /// </summary>
    private class ConditionalCamelCaseConvention : IMemberMapConvention
    {
        public string Name => "ConditionalCamelCase";

        public void Apply(BsonMemberMap memberMap)
        {
            // Ne pas appliquer camelCase aux types qui implémentent ISagaData
            if (typeof(ISagaData).IsAssignableFrom(memberMap.ClassMap.ClassType))
            {
                // Pour les sagas, mapper explicitement Id -> _id, mais garder Revision tel quel
                if (memberMap.MemberName == "Id")
                {
                    memberMap.SetElementName("_id");
                }
                // Revision reste "Revision" (pas de changement)
                return;
            }

            // Appliquer camelCase aux autres types
            var elementName = char.ToLowerInvariant(memberMap.MemberName[0]) + memberMap.MemberName.Substring(1);
            memberMap.SetElementName(elementName);
        }
    }
}

