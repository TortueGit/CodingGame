using System;
using System.Collections.Generic;
using System.Linq;

namespace CodingGame.Contest.FallChallenge.src.view
{
    public class Serializer 
    {
        static private String nullString = "_";
        static private String mainSeparator = "\n";
        static private String defaultSeparator = " ";

        static private String join(String separator, IList<int> stream)
        {
            return String.Join(separator, stream);
        }

        static private String join(IList<int> stream) 
        {
            return join(defaultSeparator, stream);
        }

        static private String join<T>(String separator, List<T> stream)
        {
            return String.Join(separator, stream.Select(obj => obj == null ? nullString : Convert.ToString(obj)));
        }

        static private String join(String separator, params object[] args) 
        {
            return join(separator, args);
        }

        static private String join(params object[] args) 
        {
            return join(defaultSeparator, args);
        }

        static private String join<T>(List<T> collection) 
        {
            return join(defaultSeparator, collection);
        }
        
        static private String joinMap<T, U>(String separator, Dictionary<T, U> map)
        {
            return String.Join(separator, new HashSet<KeyValuePair<T, U>>(map));
        }

        static private String joinMap<T>(Dictionary<T, T> map) 
        {
            return joinMap(defaultSeparator, map);
        }

        static private String animsSerialize(List<AnimationData> anims) 
        {
            if (anims == null || anims.Count <= 0) {
                return "0";
            }
            
            return join(
                anims.Count,
                String.Join(
                    defaultSeparator, 
                    anims.Select(
                        anim => join(
                            anim.Start, 
                            anim.End, 
                            anim.Trigger, 
                            anim.TriggerEnd
                        )
                    )
                )
            );
        }

        static private String spellsSerialize(List<SpellData> spells) 
        {
            if (spells == null || spells.Count <= 0) {
                return "0";
            }

            return join(
                spells.Count,
                String.Join(
                    defaultSeparator, 
                    spells.Select(
                        spell => join(
                            spell.Id, 
                            spell.Delta, 
                            spell.Repeatable, 
                            spell.Score
                        )
                    )
                )
            );
        }

        static private String spellsListsSerialize(List<List<SpellData>> spellsLists) 
        {
            if (spellsLists == null || spellsLists.Count <= 0) {
                return "0";
            }

            return join(
                mainSeparator,
                spellsLists.Count,
                String.Join(
                    mainSeparator, 
                    spellsLists.Select(
                        spells => spellsSerialize(spells)
                    )
                )
            );
        }

        static private String eventsSerialize(List<EventData> events) 
        {
            if (events == null || events.Count == 0) 
            {
                return "0";
            }            
            
            return join(
                mainSeparator,
                events.Count,
                String.Join(
                    mainSeparator,
                    events.Select(
                        x => join(
                            mainSeparator,
                            join(
                                x.Type,
                                x.PlayerIdx,
                                x.SpellId,
                                x.ResultId,
                                x.TomeIdx,
                                x.Acquired,
                                x.Lost
                            ),
                            spellsSerialize(x.Spells),
                            animsSerialize(x.AnimData)
                        )
                    )
                )
            );
        }

        static private String playerSpellsSerialize(List<List<int?>> playerSpells) 
        {
            if (playerSpells == null || playerSpells.Count == 0)
            {
                return "0";
            }

            return join(
                mainSeparator,
                playerSpells.Count,
                String.Join(mainSeparator, playerSpells.Select(spellsIds => join(spellsIds))));
        }

        static private String inventoriesSerialize(List<int[]> inventories) 
        {
            if (inventories == null || inventories.Count == 0) 
            {
                return "0";
            }

            return join(
                mainSeparator,
                inventories.Count,
                inventories.Select(inventory => join(inventory)));
        }

        private static Object bonusSerialize(Dictionary<int?, BonusData> bonus)
        {
            return String.Join(
                defaultSeparator, 
                new HashSet<KeyValuePair<int?, BonusData>>(bonus).Select(
                    entry => join(
                        defaultSeparator, 
                        entry.Key, 
                        join(
                            ",", 
                            entry.Value.Value, 
                            entry.Value.Amount
                        )
                    )
                )
            );
        }

        static public String serialize(FrameViewData frameViewData) 
        {
            return join(
                mainSeparator,
                eventsSerialize(frameViewData.Events),
                join(frameViewData.Scores),
                playerSpellsSerialize(frameViewData.PlayerSpells),
                join(frameViewData.TomeSpells),
                join(frameViewData.Deliveries),
                inventoriesSerialize(frameViewData.Inventories),
                join(frameViewData.Active),
                joinMap(frameViewData.Stock),
                bonusSerialize(frameViewData.Bonus),
                joinMap(mainSeparator, frameViewData.Messages)
            );
        }

        static public String serialize(GlobalViewData globalViewData) 
        {
            return join(
                mainSeparator,
                spellsListsSerialize(globalViewData.PlayerSpells),
                spellsSerialize(globalViewData.TomeSpells),
                spellsSerialize(globalViewData.Deliveries)
            );
        }
    }
}
