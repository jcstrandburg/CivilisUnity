using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

[Serializable]
public class TechManager {
    [DontSaveField]
    [NonSerialized]
    public Technology[] techs;
    public List<string> researchedTechs = new List<String>();

    public void LoadArray(Technology[] techs) {
        this.techs = techs;
    }

    public void LoadTree(string[] jsonStrings) {
        techs = new Technology[jsonStrings.Length];
        int x = 0;
        foreach (string s in jsonStrings) {
            try {
                techs[x] = JsonUtility.FromJson<Technology>(s);
            } catch (ArgumentException e) {
                Debug.Log(e);
                Debug.Log("Error parsing technology file: " + s);
            }
            x++;
        }
    }

    public bool TechResearched(string techname) {
        return researchedTechs.Contains(techname);
    }

    public float BuyTech(string techName) {
        if (researchedTechs.Contains(techName)) {
            return 0.0f;
        }
        foreach (Technology t in techs) {
            if (t.techName == techName) {
                researchedTechs.Add(techName);
                return t.cost;
            }
        }
        return 0.0f;
    }

    public bool PrereqsMet(Technology t) {
        foreach (String req in t.requires) {
            if (!researchedTechs.Contains(req)) {
                return false;
            }
        }
        return true;
    }

    public Technology[] GetEligibleTechs() {
        List<Technology> elligibles = new List<Technology>();
        foreach (Technology t in techs) {
            if (!researchedTechs.Contains(t.techName) && PrereqsMet(t)) {
                elligibles.Add(t);
            }
        }
        return elligibles.ToArray();
    }

    public void Research(Technology t) {
        if (researchedTechs.Contains(t.techName)) {
            throw new Exception("Technology already researched: " + t.techName);
        }
        if (!techs.Contains<Technology>(t)) {
            throw new Exception("Unable to research tech: " + t.techName);
        }
        researchedTechs.Add(t.techName);
    }
}
