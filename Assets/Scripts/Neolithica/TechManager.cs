using System;
using System.Collections.Generic;
using System.Linq;
using AqlaSerializer;
using Neolithica.ScriptableObjects;
using Tofu.Serialization;
using UnityEngine;

namespace Neolithica {
    [Serializable, SerializableType]
    public class TechManager {

        public Technology[] Techs;
        [SerializableMember(1)] public List<string> ResearchedTechs = new List<string>();

        public void LoadTechs(IEnumerable<Technology> techs) {
            Techs = techs.ToArray();
        }

        public void LoadTree(string[] jsonStrings) {
            Techs = new Technology[jsonStrings.Length];
            int x = 0;
            foreach (string s in jsonStrings) {
                try {
                    Techs[x] = JsonUtility.FromJson<Technology>(s);
                } catch (ArgumentException e) {
                    Debug.Log(e);
                    Debug.Log($"Error parsing technology file: {s}");
                }
                x++;
            }
        }

        public bool TechResearched(string techname) {
            return ResearchedTechs.Contains(techname);
        }

        public float BuyTech(string techName) {
            if (ResearchedTechs.Contains(techName)) {
                return 0.0f;
            }
            foreach (Technology t in Techs) {
                if (t.techName == techName) {
                    ResearchedTechs.Add(techName);
                    return t.cost;
                }
            }
            return 0.0f;
        }

        public bool PrereqsMet(Technology t) {
            foreach (String req in t.requires) {
                if (!ResearchedTechs.Contains(req)) {
                    return false;
                }
            }
            return true;
        }

        public Technology[] GetEligibleTechs() {
            List<Technology> elligibles = new List<Technology>();
            foreach (Technology t in Techs) {
                if (!ResearchedTechs.Contains(t.techName) && PrereqsMet(t)) {
                    elligibles.Add(t);
                }
            }
            return elligibles.ToArray();
        }

        public void Research(Technology t) {
            if (ResearchedTechs.Contains(t.techName)) {
                throw new InvalidOperationException($"Technology already researched: {t.techName}");
            }
            if (!Techs.Contains<Technology>(t)) {
                throw new Exception($"Unable to research tech: {t.techName}");
            }
            ResearchedTechs.Add(t.techName);
        }
    }
}
