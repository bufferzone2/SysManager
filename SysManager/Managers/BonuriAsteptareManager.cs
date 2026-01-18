//Fisier:BonuriAsteptareManager.cs
using System;
using System.Collections.Generic;
using FirebirdSql.Data.FirebirdClient;
using SysManager.Models;

namespace SysManager.Managers
{
    public class BonuriAsteptareManager
    {
        /// <summary>
        /// Constructor - NU mai primește connection string
        /// </summary>
        public BonuriAsteptareManager()
        {
            // Folosim DbConnectionFactory pentru conexiuni
        }

        /// <summary>
        /// Salvează bonul curent în așteptare
        /// </summary>
        public int SalveazaBonInAsteptare(BonAsteptare bon)
        {
            using (var conn = DbConnectionFactory.GetOpenConnection())
            {
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Inserează header (RĂMÂNE NESCHIMBAT)
                        string sqlHeader = @"
                    INSERT INTO BONURI_ASTEPTARE 
                    (NR_BON, DATA_CREARE, ID_UTILIZATOR, ID_GESTIUNE, TOTAL, OBSERVATII, NUME_CLIENT, STATUS)
                    VALUES (@NrBon, @DataCreare, @IdUtilizator, @IdGestiune, @Total, @Observatii, @NumeClient, @Status)
                    RETURNING ID";

                        int bonId;
                        using (var cmd = new FbCommand(sqlHeader, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@NrBon", bon.NrBon);
                            cmd.Parameters.AddWithValue("@DataCreare", bon.DataCreare);
                            cmd.Parameters.AddWithValue("@IdUtilizator", bon.IdUtilizator);
                            cmd.Parameters.AddWithValue("@IdGestiune", bon.IdGestiune);
                            cmd.Parameters.AddWithValue("@Total", bon.Total);
                            cmd.Parameters.AddWithValue("@Observatii", bon.Observatii ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@NumeClient", bon.NumeClient ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Status", "ASTEPTARE");

                            bonId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // ✅ 2. MODIFICĂ AICI - adaugă ESTE_GARANTIE în INSERT
                        string sqlDetalii = @"
                    INSERT INTO BONURI_ASTEPTARE_DETALII 
                    (ID_BON_ASTEPTARE, ID_PRODUS, DENUMIRE_PRODUS, CANTITATE, PRET_UNITAR, VALOARE, ESTE_GARANTIE)
                    VALUES (@IdBon, @IdProdus, @Denumire, @Cantitate, @PretUnitar, @Valoare, @EsteGarantie)";

                        foreach (var detaliu in bon.Detalii)
                        {
                            using (var cmd = new FbCommand(sqlDetalii, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@IdBon", bonId);
                                cmd.Parameters.AddWithValue("@IdProdus", detaliu.IdProdus);
                                cmd.Parameters.AddWithValue("@Denumire", detaliu.DenumireProdus);
                                cmd.Parameters.AddWithValue("@Cantitate", detaliu.Cantitate);
                                cmd.Parameters.AddWithValue("@PretUnitar", detaliu.PretUnitar);
                                cmd.Parameters.AddWithValue("@Valoare", detaliu.Valoare);
                                cmd.Parameters.AddWithValue("@EsteGarantie", detaliu.EsteGarantie ? 1 : 0);  // ← ADAUGĂ
                                cmd.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                        Logs.Write($"✅ Bon #{bon.NrBon} salvat în așteptare (ID: {bonId})");
                        return bonId;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        Logs.Write($"❌ EROARE salvare bon în așteptare: {ex.Message}");
                        Logs.Write(ex);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Obține toate bonurile în așteptare
        /// </summary>
        public List<BonAsteptare> GetBonuriInAsteptare()
        {
            var bonuri = new List<BonAsteptare>();

            using (var conn = DbConnectionFactory.GetOpenConnection()) // ✅ FOLOSEȘTE DbConnectionFactory
            {
                string sql = @"
                    SELECT ID, NR_BON, DATA_CREARE, ID_UTILIZATOR, ID_GESTIUNE, 
                           TOTAL, OBSERVATII, NUME_CLIENT, STATUS
                    FROM BONURI_ASTEPTARE
                    WHERE STATUS = 'ASTEPTARE'
                    ORDER BY DATA_CREARE DESC";

                using (var cmd = new FbCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var bon = new BonAsteptare
                        {
                            Id = reader.GetInt32(0),
                            NrBon = reader.GetString(1),
                            DataCreare = reader.GetDateTime(2),
                            IdUtilizator = reader.GetInt32(3),
                            IdGestiune = reader.GetInt32(4),
                            Total = reader.GetDecimal(5),
                            Observatii = reader.IsDBNull(6) ? null : reader.GetString(6),
                            NumeClient = reader.IsDBNull(7) ? null : reader.GetString(7),
                            Status = reader.GetString(8)
                        };

                        bonuri.Add(bon);
                    }
                }
            }

            Logs.Write($"✅ Încărcate {bonuri.Count} bonuri în așteptare");
            return bonuri;
        }

        /// <summary>
        /// Încarcă un bon din așteptare cu toate detaliile
        /// </summary>
        public BonAsteptare IncarcaBon(int idBon)
        {
            BonAsteptare bon = null;

            using (var conn = DbConnectionFactory.GetOpenConnection())
            {
                // 1. Încarcă header (RĂMÂNE NESCHIMBAT)
                string sqlHeader = @"
            SELECT ID, NR_BON, DATA_CREARE, ID_UTILIZATOR, ID_GESTIUNE, 
                   TOTAL, OBSERVATII, NUME_CLIENT, STATUS
            FROM BONURI_ASTEPTARE
            WHERE ID = @Id";

                using (var cmd = new FbCommand(sqlHeader, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", idBon);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bon = new BonAsteptare
                            {
                                Id = reader.GetInt32(0),
                                NrBon = reader.GetString(1),
                                DataCreare = reader.GetDateTime(2),
                                IdUtilizator = reader.GetInt32(3),
                                IdGestiune = reader.GetInt32(4),
                                Total = reader.GetDecimal(5),
                                Observatii = reader.IsDBNull(6) ? null : reader.GetString(6),
                                NumeClient = reader.IsDBNull(7) ? null : reader.GetString(7),
                                Status = reader.GetString(8)
                            };
                        }
                    }
                }

                if (bon == null)
                {
                    Logs.Write($"❌ Bonul cu ID {idBon} nu a fost găsit");
                    return null;
                }

                // ✅ 2. MODIFICĂ AICI - adaugă ESTE_GARANTIE în SELECT
                string sqlDetalii = @"
            SELECT ID, ID_PRODUS, DENUMIRE_PRODUS, CANTITATE, PRET_UNITAR, VALOARE, ESTE_GARANTIE
            FROM BONURI_ASTEPTARE_DETALII
            WHERE ID_BON_ASTEPTARE = @IdBon
            ORDER BY ID";  // ← Important pentru ordinea corectă

                using (var cmd = new FbCommand(sqlDetalii, conn))
                {
                    cmd.Parameters.AddWithValue("@IdBon", idBon);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bon.Detalii.Add(new BonAsteptareDetaliu
                            {
                                Id = reader.GetInt32(0),
                                IdBonAsteptare = idBon,
                                IdProdus = reader.GetInt32(1),
                                DenumireProdus = reader.GetString(2),
                                Cantitate = reader.GetDecimal(3),
                                PretUnitar = reader.GetDecimal(4),
                                Valoare = reader.GetDecimal(5),
                                EsteGarantie = reader.IsDBNull(6) ? false : reader.GetInt16(6) == 1  // ← ADAUGĂ
                            });
                        }
                    }
                }
            }

            Logs.Write($"✅ Bon #{bon.NrBon} încărcat cu {bon.Detalii.Count} linii");
            return bon;
        }


        /// <summary>
        /// Șterge bonul din așteptare
        /// </summary>
        public void StergeBon(int idBon)
        {
            using (var conn = DbConnectionFactory.GetOpenConnection()) // ✅ FOLOSEȘTE DbConnectionFactory
            {
                string sql = "DELETE FROM BONURI_ASTEPTARE WHERE ID = @Id";
                using (var cmd = new FbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", idBon);
                    cmd.ExecuteNonQuery();
                }
            }

            Logs.Write($"✅ Bon ID {idBon} șters din așteptare");
        }

        /// <summary>
        /// Marchează bonul ca închis
        /// </summary>
        public void MarcheazaBonInchis(int idBon)
        {
            using (var conn = DbConnectionFactory.GetOpenConnection()) // ✅ FOLOSEȘTE DbConnectionFactory
            {
                string sql = "UPDATE BONURI_ASTEPTARE SET STATUS = 'INCHIS' WHERE ID = @Id";
                using (var cmd = new FbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", idBon);
                    cmd.ExecuteNonQuery();
                }
            }

            Logs.Write($"✅ Bon ID {idBon} marcat ca închis");
        }
    }
}
