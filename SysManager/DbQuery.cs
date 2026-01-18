// DbQuery.cs
using System;
using System.Collections.Generic;
using FirebirdSql.Data.FirebirdClient;
using System.Linq;
using SysManager.Models;

namespace SysManager
{
    public class DbQuery
    {
        public IEnumerable<(int Id, string Name, decimal Price)> GetAll()
        {
            var list = new List<(int, string, decimal)>();

            try
            {
                using (var conn = DbConnectionFactory.GetOpenConnection())
                using (var cmd = new FbCommand("SELECT ID, NAME, PRICE FROM PRODUSE", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add((reader.GetInt32(0), reader.GetString(1), reader.GetDecimal(2)));
                    }
                }

                //Logs.Write($"GetAll: Încărcate {list.Count} produse");
            }
            catch (Exception ex)
            {
                Logs.Write(ex);
                throw;
            }

            return list;
        }

        public void Add(string name, decimal price)
        {
            try
            {
                using (var conn = DbConnectionFactory.GetOpenConnection())
                using (var cmd = new FbCommand("INSERT INTO PRODUSE (NAME, PRICE) VALUES (@name, @price)", conn))
                {
                    cmd.Parameters.AddWithValue("name", name);
                    cmd.Parameters.AddWithValue("price", price);
                    cmd.ExecuteNonQuery();
                }

                //Logs.Write($"Add: Produs adăugat - {name}, {price} RON");
            }
            catch (Exception ex)
            {
                Logs.Write(ex);
                throw;
            }
        }

        /// <summary>
        /// Preia grupele de articole din baza de date folosind procedura GET_GRUPE_ARTICOLE
        /// </summary>
        public List<GrupaArticole> GetGrupeArticole(int gestiuneId = 0, int useShowOrder = 1)
        {
            var grupe = new List<GrupaArticole>();

            try
            {
                //Logs.Write($"GetGrupeArticole: Încărcare grupe (gestiuneId={gestiuneId}, useShowOrder={useShowOrder})");

                using (var conn = DbConnectionFactory.GetOpenConnection())
                using (var cmd = new FbCommand("SELECT * FROM GET_GRUPE_ARTICOLE(?, ?)", conn))
                {
                    cmd.Parameters.Add(new FbParameter { Value = gestiuneId });
                    cmd.Parameters.Add(new FbParameter { Value = useShowOrder });

                    using (var reader = cmd.ExecuteReader())
                    {
                        int index = 0;
                        while (reader.Read())
                        {
                            try
                            {
                                string denumire = reader.IsDBNull(0) ? "N/A" : reader.GetString(0);
                                int numarArticole = reader.IsDBNull(1) ? 0 : reader.GetInt32(1); // ✅ Acum citim direct ca INT32
                                int id = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                                int showOrder = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);

                                var grupa = new GrupaArticole
                                {
                                    Denumire = denumire,
                                    NumarArticole = numarArticole,
                                    Id = id,
                                    ShowOrder = showOrder
                                };

                                //Logs.Write($"  [{++index}] '{grupa.Denumire}' → {grupa.NumarArticole} articole");
                                grupe.Add(grupa);
                            }
                            catch (Exception ex)
                            {
                                Logs.Write($"❌ EROARE citire rând {index + 1}:");
                                Logs.Write(ex);
                            }
                        }
                    }
                }

                //Logs.Write($"✅ Încărcate {grupe.Count} grupe (Total articole: {grupe.Sum(g => g.NumarArticole)})");
            }
            catch (Exception ex)
            {
                Logs.Write($"❌ EROARE GetGrupeArticole:");
                Logs.Write(ex);
                throw;
            }

            return grupe;
        }


        /// <summary>
        /// Încarcă setările pentru afișarea grupelor (dimensiuni butoane, înălțime panou, culori)
        /// </summary>
        public GrupeSettings GetGrupeSettings()
        {
            var settings = new GrupeSettings
            {
                Id = 1,
                Inaltime = 75,
                Latime = 150,
                PanouHeight = 0 // 0 = automat
            };

            try
            {
                //Logs.Write("GetGrupeSettings: Încărcare setări grupe");

                using (var conn = DbConnectionFactory.GetOpenConnection())
                using (var cmd = new FbCommand(@"
                    SELECT ID, INALTIME, LATIME, PANOU_HEIGHT, 
                           BORDER_COLOR, COLOR_NORMAL, COLOR_PRESSED, 
                           COLOR_HOVER, COLOR_DISABLED
                    FROM SM_GRUPE_SETTINGS
                    WHERE ID = 1", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        settings.Id = reader.GetInt32(0);

                        // Parse INALTIME și LATIME (sunt VARCHAR)
                        if (int.TryParse(reader.GetString(1), out int inaltime))
                            settings.Inaltime = inaltime;

                        if (int.TryParse(reader.GetString(2), out int latime))
                            settings.Latime = latime;

                        settings.PanouHeight = reader.GetInt32(3);

                        settings.BorderColor = reader.IsDBNull(4) ? null : reader.GetString(4);
                        settings.ColorNormal = reader.IsDBNull(5) ? null : reader.GetString(5);
                        settings.ColorPressed = reader.IsDBNull(6) ? null : reader.GetString(6);
                        settings.ColorHover = reader.IsDBNull(7) ? null : reader.GetString(7);
                        settings.ColorDisabled = reader.IsDBNull(8) ? null : reader.GetString(8);

                        //Logs.Write($"GetGrupeSettings: Încărcate setări - Buton: {settings.Latime}x{settings.Inaltime}px, Panou: {settings.PanouHeight}px");
                    }
                    else
                    {
                        //Logs.Write("GetGrupeSettings: Nicio înregistrare găsită, folosim valorile default");
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Write("EROARE la încărcarea setărilor grupelor:");
                Logs.Write(ex);
            }

            return settings;
        }


        /// <summary>
        /// Încarcă setările pentru afișarea produselor (dimensiuni butoane, înălțime panou, culori)
        /// </summary>
        public ProduseSettings GetProduseSettings()
        {
            var settings = new ProduseSettings
            {
                Id = 1,
                Inaltime = 75,
                Latime = 150,
            };

            try
            {
                //Logs.Write("GetPruseSettings: Încărcare setări produse");

                using (var conn = DbConnectionFactory.GetOpenConnection())
                using (var cmd = new FbCommand(@"
                    SELECT ID, INALTIME, LATIME, BORDER_COLOR, COLOR_NORMAL, COLOR_PRESSED, 
                           COLOR_HOVER, COLOR_DISABLED
                    FROM SM_PROD_SETTINGS
                    WHERE ID = 1", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        settings.Id = reader.GetInt32(0);

                        // Parse INALTIME și LATIME (sunt VARCHAR)
                        if (int.TryParse(reader.GetString(1), out int inaltime))
                            settings.Inaltime = inaltime;

                        if (int.TryParse(reader.GetString(2), out int latime))
                            settings.Latime = latime;


                        settings.BorderColor = reader.IsDBNull(3) ? null : reader.GetString(3);
                        settings.ColorNormal = reader.IsDBNull(4) ? null : reader.GetString(4);
                        settings.ColorHover = reader.IsDBNull(5) ? null : reader.GetString(5);
                        settings.ColorPressed = reader.IsDBNull(6) ? null : reader.GetString(6);                       
                        settings.ColorDisabled = reader.IsDBNull(7) ? null : reader.GetString(7);

                        //Logs.Write($"GetPruseSettings: Încărcate setări - Buton: {settings.Latime}x{settings.Inaltime}px");
                    }
                    else
                    {
                        //Logs.Write("GetPruseSettings: Nicio înregistrare găsită, folosim valorile default");
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Write("EROARE la încărcarea setărilor produselor:");
                Logs.Write(ex);
            }

            return settings;
        }

        /// <summary>
        /// Încarcă lista de gestiuni active din baza de date
        /// </summary>
        public List<Gestiune> GetGestiuni()
        {
            var gestiuni = new List<Gestiune>();

            try
            {
                //Logs.Write("GetGestiuni: Încărcare gestiuni active");

                using (var conn = DbConnectionFactory.GetOpenConnection()) // ✅ FOLOSEȘTE DbConnectionFactory
                using (var cmd = new FbCommand(@"
            SELECT ID, NUME, NUME_GEST, STATUS
            FROM SM_GESTIUNI
            WHERE STATUS = 1
            ORDER BY ID", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        gestiuni.Add(new Gestiune
                        {
                            Id = reader.GetInt32(0),
                            Nume = reader.IsDBNull(1) ? "" : reader.GetString(1).Trim(),
                            NumeGest = reader.IsDBNull(2) ? "" : reader.GetString(2).Trim(),
                            Status = reader.GetInt32(3)
                        });
                    }
                }

                //Logs.Write($"✅ Încărcate {gestiuni.Count} gestiuni din baza de date");
            }
            catch (Exception ex)
            {
                Logs.Write("❌ EROARE la încărcarea gestiunilor:");
                Logs.Write(ex);
            }

            return gestiuni;
        }

        /// <summary>
        /// Încarcă produsele pentru o grupă și gestiune specifică
        /// </summary>
        /// <param name="grupaId">ID-ul grupei (0 sau NULL = toate grupele)</param>
        /// <param name="gestiuneId">ID-ul gestiunii (0 = toate gestiunile)</param>
        /// <returns>Lista de produse</returns>
        public List<Produs> GetProduse(int gestiuneId = 0, int grupaId = 0)
        {
            var produse = new List<Produs>();

            try
            {
                // Convertim 0 în NULL pentru MY_PARAM (grupaId)
                object myParam = grupaId == 0 ? (object)DBNull.Value : grupaId;

                //Logs.Write($"GetProduse: Încărcare produse (grupaId={grupaId}, gestiuneId={gestiuneId})");

                using (var conn = DbConnectionFactory.GetOpenConnection())
                using (var cmd = new FbCommand("SELECT * FROM CITESTEARTICOLE_GRUPA_GESTIUNE(?, ?)", conn))
                {
                    // ✅ Parametri poziționali
                    cmd.Parameters.Add(new FbParameter { Value = myParam });          // MY_PARAM (grupa)
                    cmd.Parameters.Add(new FbParameter { Value = gestiuneId });       // GESTIUNE_PARAM

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            produse.Add(new Produs
                            {
                                Id = reader.GetInt32(0),                                                    // ID
                                Denumire = reader.GetString(1).Trim(),                                      // NUME
                                ValoareTva = reader.GetDecimal(2),                                          // VALOARE TVA PRODUS
                                PretBrut = reader.GetDecimal(3),                                            // BRUT
                                ProcentTva = reader.GetDecimal(4),                                          // PROCENTUL TVA
                                CaleImagine = reader.IsDBNull(5) ? null : reader.GetString(5).Trim(),       // CALE_IMAGE
                                ShowImage = reader.GetInt32(6),                                             // SHOW_IMAGE
                                TvaId = reader.GetInt32(7),                                                 // TVA_ID
                                CodSGR = reader.IsDBNull(8) ? null : reader.GetString(8).Trim(),            // COD_SGR
                                UnitateMasura = reader.IsDBNull(9) ? "buc" : reader.GetString(9).Trim(),    // U_MASURA
                                Departament = reader.GetInt32(10),                                          // ID_DEP
                                TvaAmefId = reader.GetInt32(11),                                            // ID_TVA_AMEF
                                NumeGestiune = reader.GetString(13).Trim()                                  // NUME GESTIUNE
                            });
                        }
                    }
                }

                //Logs.Write($"GetProduse: Încărcate {produse.Count} produse cu succes");
            }
            catch (Exception ex)
            {
                Logs.Write("EROARE la încărcarea produselor:");
                Logs.Write(ex);
                //Logs.Write($"GetProduse: EROARE (grupaId={grupaId}, gestiuneId={gestiuneId})");
                // Nu aruncăm excepția mai departe pentru a nu bloca interfața
            }

            return produse;
        }

        /// <summary>
        /// Caută produse după nume folosind procedura stocată SEARCHARTICOLE
        /// </summary>
        /// <param name="searchText">Textul de căutat</param>
        /// <param name="grupaId">ID-ul grupei (0 = toate produsele)</param>
        /// <returns>Lista de produse găsite</returns>
        public List<Produs> SearchArticole(string searchText, int grupaId)
        {
            var produse = new List<Produs>();

            try
            {
                // ✅ Convertim 0 în NULL pentru PARAM_GRUPA
                object grupaParam = grupaId == 0 ? (object)DBNull.Value : grupaId;

                //Logs.Write($"🔍 SearchArticole: Căutare '{searchText}' (grupaId={grupaId})");

                // ✅ FOLOSEȘTE DbConnectionFactory (ca toate celelalte metode!)
                using (var conn = DbConnectionFactory.GetOpenConnection())
                using (var cmd = new FbCommand("SELECT * FROM SEARCHARTICOLE(?, ?)", conn))
                {
                    // ✅ Parametri poziționali (ca în GetProduse)
                    cmd.Parameters.Add(new FbParameter { Value = searchText ?? "" });  // PARAM_ARTICOL
                    cmd.Parameters.Add(new FbParameter { Value = grupaParam });         // PARAM_GRUPA

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var produs = new Produs
                            {
                                Id = reader.GetInt32(0),                                    // ID
                                Denumire = reader.GetString(1).Trim(),                      // NUME
                                ValoareTva = reader.GetDecimal(2),                          // VALOARE TVA PRODUS
                                PretBrut = reader.GetDecimal(3),                            // BRUT
                                ProcentTva = reader.GetDecimal(4),                          // PROCENTUL TVA
                                CaleImagine = reader.IsDBNull(5) ? null : reader.GetString(5).Trim(), // CALE_IMAGE
                                ShowImage = reader.GetInt32(6),                             // SHOW_IMAGE (✅ INT32, nu INT16!)
                                TvaId = reader.GetInt32(7),                                 // TVA_ID
                                CodSGR = reader.IsDBNull(8) ? null : reader.GetString(8).Trim(),     // COD_SGR
                                UnitateMasura = reader.IsDBNull(9) ? "buc" : reader.GetString(9).Trim(), // U_MASURA
                                Departament = reader.GetInt32(10),                          // ID_DEP (✅ INT32, nu INT16!)
                                TvaAmefId = reader.GetInt32(11),                             // ID_TVA_AMEF (✅ INT32, nu INT16!)
                                NumeGestiune = reader.GetString(13).Trim()                  // NUME GESTIUNE
                            };

                            produse.Add(produs);
                        }
                    }
                }

                //Logs.Write($"✅ SearchArticole: Găsite {produse.Count} produse pentru '{searchText}' (Grupa: {(grupaId == 0 ? "Toate" : grupaId.ToString())})");
            }
            catch (Exception ex)
            {
                Logs.Write("❌ EROARE la căutarea produselor:");
                Logs.Write(ex);
                // ✅ Nu aruncăm excepția mai departe pentru a nu bloca interfața
            }

            return produse;
        }

        /// <summary>
        /// Încarcă un produs specific după ID folosind procedura SEARCH_ART_GRUPA
        /// Folosit pentru încărcarea produsului garanție SGR
        /// </summary>
        /// <param name="idProdus">ID-ul produsului din baza de date</param>
        /// <returns>Produsul sau null dacă nu există</returns>
        public Produs GetProdusDupaId(int idProdus)
        {
            try
            {
                //Logs.Write($"GetProdusDupaId: Încărcare produs cu ID={idProdus}");

                using (var conn = DbConnectionFactory.GetOpenConnection())
                using (var cmd = new FbCommand("SELECT * FROM SEARCH_ART_GRUPA(?, ?, ?)", conn))
                {
                    // ✅ PARAMETRI PROCEDURII SEARCH_ART_GRUPA
                    cmd.Parameters.Add(new FbParameter { Value = idProdus });       // PARAM_ID
                    cmd.Parameters.Add(new FbParameter { Value = DBNull.Value });   // PARAM_ARTICOL (null)
                    cmd.Parameters.Add(new FbParameter { Value = DBNull.Value });   // PARAM_GRUPA (null)

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var produs = new Produs
                            {
                                Id = reader.GetInt32(0),                                                    // ID
                                Denumire = reader.GetString(1).Trim(),                                      // NUME
                                ValoareTva = reader.GetDecimal(2),                                          // VALOARE TVA PRODUS
                                PretBrut = reader.GetDecimal(3),                                            // BRUT
                                ProcentTva = reader.GetDecimal(4),                                          // PROCENTUL TVA
                                CaleImagine = reader.IsDBNull(5) ? null : reader.GetString(5).Trim(),       // CALE_IMAGE
                                ShowImage = reader.GetInt32(6),                                             // SHOW_IMAGE
                                TvaId = reader.GetInt32(7),                                                 // TVA_ID
                                CodSGR = reader.IsDBNull(8) ? null : reader.GetString(8).Trim(),            // COD_SGR
                                UnitateMasura = reader.IsDBNull(9) ? "buc" : reader.GetString(9).Trim(),    // U_MASURA
                                Departament = reader.GetInt32(10),                                          // ID_DEP
                                TvaAmefId = reader.GetInt32(11)                                             // ID_TVA_AMEF
                            };

                            // ✅ CALCULEAZĂ ProcentTva din ValoareTva și Pret
                            //if (produs.Pret > 0)
                            //{
                            //    produs.ProcentTva = (produs.ProcentTva / produs.Pret) * 100;
                            //}
                            //else
                            //{
                            //    produs.ProcentTva = 0;
                            //}

                            //Logs.Write($"✅ GetProdusDupaId: Produs găsit - ID={produs.Id}, Denumire='{produs.Denumire}', Pret={produs.PretBrut:F2} RON");
                            return produs;
                        }
                        else
                        {
                            //Logs.Write($"⚠️ GetProdusDupaId: Niciun produs găsit cu ID={idProdus}");
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Write($"❌ EROARE GetProdusDupaId (ID={idProdus}):");
                Logs.Write(ex);
                return null;
            }
        }

        /// <summary>
        /// Încarcă configurația generală a aplicației din tabela SM_CONFIG
        /// </summary>
        /// <returns>Obiect SmConfig sau null dacă nu există</returns>
        public SmConfig GetConfig()
        {
            try
            {
                //Logs.Write("GetConfig: Încărcare configurație generală din SM_CONFIG");

                using (var conn = DbConnectionFactory.GetOpenConnection())
                using (var cmd = new FbCommand(@"
            SELECT 
                ID,
                IMPRIMANTA_NOTA,
                ENABLED_SOUND,
                SELL_NEGATIVE_STOCK,
                CUMULEAZA_ARTICOLE_VANDUTE,
                DEL_COM_NOTA_PRINT,
                IMPRIMANTA_IMPLICITA,
                IMPRIMANTA_IMPLICITA_NOTA,
                ENABLED_SGR
            FROM SM_CONFIG
            WHERE ID = 1", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var config = new SmConfig
                        {
                            Id = reader.GetInt16(0),                                                    // ID
                            ImprimantaNota = reader.IsDBNull(1) ? null : reader.GetString(1).Trim(),  // IMPRIMANTA_NOTA
                            EnabledSound = reader.IsDBNull(2) ? (short)1 : reader.GetInt16(2),        // ENABLED_SOUND
                            SellNegativeStock = reader.IsDBNull(3) ? (short)1 : reader.GetInt16(3),   // SELL_NEGATIVE_STOCK
                            CumuleazaArticoleVandute = reader.IsDBNull(4) ? (short)1 : reader.GetInt16(4), // CUMULEAZA_ARTICOLE_VANDUTE
                            DelComNotaPrint = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),            // DEL_COM_NOTA_PRINT
                            ImprimantaImplicita = reader.IsDBNull(6) ? null : reader.GetString(6).Trim(), // IMPRIMANTA_IMPLICITA
                            ImprimantaImplicitaNota = reader.IsDBNull(7) ? null : reader.GetString(7).Trim(), // IMPRIMANTA_IMPLICITA_NOTA
                            EnabledSGR = reader.GetInt16(8)                                            // ENABLED_SGR (NOT NULL)
                        };

                        //Logs.Write($"✅ GetConfig: Configurație încărcată - SGR={(config.IsSGREnabled ? "ACTIVAT" : "DEZACTIVAT")}, " +
                                  //$"Sound={(config.IsSoundEnabled ? "ON" : "OFF")}, " +
                                  //$"NegativeStock={(config.CanSellNegativeStock ? "DA" : "NU")}");

                        return config;
                    }
                    else
                    {
                        //Logs.Write("⚠️ GetConfig: Nicio configurație găsită cu ID=1 în SM_CONFIG");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Write("❌ EROARE GetConfig:");
                Logs.Write(ex);
                return null;
            }
        }
    }
}
