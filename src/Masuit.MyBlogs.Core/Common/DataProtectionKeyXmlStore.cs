using Microsoft.AspNetCore.DataProtection.Repositories;
using Npgsql;
using System.Xml.Linq;

namespace Masuit.MyBlogs.Core.Common;

public sealed class DataProtectionKeyXmlStore(string connectionString) : IXmlRepository
{
    private void EnsureTableExists()
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        using var command = new NpgsqlCommand("""
            CREATE TABLE IF NOT EXISTS "DataProtectionKeys" (
                "Id" SERIAL PRIMARY KEY,
                "FriendlyName" TEXT NOT NULL UNIQUE,
                "Xml" TEXT NOT NULL
            )
            """, connection);
        command.ExecuteNonQuery();
    }

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        EnsureTableExists();
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        using var command = new NpgsqlCommand("""SELECT "Xml" FROM "DataProtectionKeys" """, connection);
        using var reader = command.ExecuteReader();
        var elements = new List<XElement>();
        while (reader.Read())
        {
            elements.Add(XElement.Parse(reader.GetString(0)));
        }

        return elements;
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        EnsureTableExists();
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        using var command = new NpgsqlCommand("""
            INSERT INTO "DataProtectionKeys" ("FriendlyName", "Xml")
            VALUES (@friendlyName, @xml)
            ON CONFLICT ("FriendlyName") DO UPDATE SET "Xml" = EXCLUDED."Xml"
            """, connection);
        command.Parameters.AddWithValue("friendlyName", friendlyName);
        command.Parameters.AddWithValue("xml", element.ToString(SaveOptions.DisableFormatting));
        command.ExecuteNonQuery();
    }
}
