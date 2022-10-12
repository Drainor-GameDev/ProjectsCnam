using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading;

namespace JeuDeCombat
{
    public class charactersActionValue
    {
        public string classe, name;
        public int damage;
        public int defence;
        public int armor;
        public int hp, maxHp;
        public bool sendBack = false, healBack = false, ceFameuxBill = false, silvered = false;
        List<Buff> buffs = new List<Buff>();

        public charactersActionValue(Tuple<string, int, int, int> _classe, string _name)
        {
            classe = _classe.Item1;
            damage = _classe.Item3;
            maxHp = _classe.Item2;
            armor = _classe.Item4;
            hp = maxHp;
            name = _name;
            if (classe == "Silver")
            {
                Buff bufftemp = new Buff(20, this);
                bufftemp.cooldown[2] = true;
                buffs.Add(bufftemp);
            }
        }

        public void Kill()
        {
            hp = 0;
        }

        public void AddHp(int heal)
        {
            hp = Math.Clamp(hp + heal, 0, maxHp);
        }

        public void SimpleAttack(charactersActionValue other)
        {
            DoDmgToOther(damage, other);
        }

        public void GetDmg(int dmg, charactersActionValue other, bool ignoreDef = false)
        {
            if (sendBack)
            {
                sendBack = false;
                other.GetDmg(dmg);
            }
            else if (healBack)
            {
                healBack = false;
                AddHp(dmg);
            }
            else
            {
                if (ignoreDef)
                {
                    hp = hp - Math.Clamp(dmg - GetDefence(), 0, dmg);
                }
                else
                {
                    if (!GetBlock())
                        hp = hp - Math.Clamp((int)(dmg * (1f - (GetArmor() / 100f))) + GetDefence(), 0, dmg);
                }
            }
        }
        public void GetDmg(int dmg, bool ignoreDef = false)
        {
            if (ignoreDef)
            {
                hp = hp - Math.Clamp(dmg - GetDefence(), 0, dmg);
            }
            else
            {
                if (!GetBlock())
                    hp = hp - Math.Clamp((int)(dmg * (1f - (GetArmor() / 100f))) + GetDefence(), 0, dmg);
            }
        }

        public void DoDmgToOther(int dmg, charactersActionValue other, bool ignoreDef = false)
        {
            int dmgtodo = dmg;
            foreach (Buff buff in buffs)
            {
                if (buff.activate)
                    dmgtodo += buff.bonusDmg;
            }
            other.GetDmg(dmgtodo, this, ignoreDef);
        }

        public string GetClass()
        {
            return classe;
        }

        public int GetHp()
        {
            return hp;
        }

        public float GetArmor()
        {
            int armortemp = armor;
            foreach (Buff buff in buffs)
            {
                if (buff.activate)
                    armortemp -= buff.armorReduce;
            }
            return armortemp;
        }

        public bool DoSpecial(charactersActionValue other, int selectedSpecial, out string texte)
        {
            //selectedSpecial--;
            if (CheckCD(selectedSpecial) == 0 && selectedSpecial != 0)
            {
                Buff bufftemp;
                switch (classe)
                {
                    case "Bestraf":
                        switch (selectedSpecial)
                        {
                            case 1:
                                sendBack = true;
                                bufftemp = new Buff(4, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 2:
                                bufftemp = new Buff(3, this, other);
                                bufftemp.bonusDmg = 50;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(7, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 3:
                                DoDmgToOther(damage, other, true);
                                bufftemp = new Buff(3, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                    case "Kolyma":
                        switch (selectedSpecial)
                        {
                            case 1:
                                AddHp(250);
                                bufftemp = new Buff(5, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 2:
                                bufftemp = new Buff(5, this, other);
                                bufftemp.bonusDmg = -30;
                                other.buffs.Add(bufftemp);
                                bufftemp = new Buff(12, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 3:
                                bufftemp = new Buff(4, this, other);
                                bufftemp.dmgToOther = 20;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(6, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                    case "Zelote":
                        switch (selectedSpecial)
                        {
                            case 1:
                                bufftemp = new Buff(4, this, other);
                                bufftemp.bonusDmg = 15;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(12, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                DoDmgToOther(damage, other);
                                GetDmg(150);
                                break;
                            case 2:
                                bufftemp = new Buff(2, this, other);
                                bufftemp.block = true;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(7, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 3:
                                bufftemp = new Buff(3, this, other);
                                bufftemp.armorReduce = 50;
                                other.buffs.Add(bufftemp);
                                bufftemp = new Buff(10, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                    case "Xyns":
                        switch (selectedSpecial)
                        {
                            case 1:
                                bufftemp = new Buff(2, this, other);
                                bufftemp.bonusDmg = damage;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(7, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 2:
                                bufftemp = new Buff(4, this, other);
                                bufftemp.defence = 30;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(9, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 3:
                                bufftemp = new Buff(2, this, other);
                                bufftemp.stun = true;
                                other.buffs.Add(bufftemp);
                                bufftemp = new Buff(15, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                    case "Daicy":
                        switch (selectedSpecial)
                        {
                            case 1:
                                bufftemp = new Buff(3, this, other);
                                bufftemp.block = true;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(10, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 2:
                                bufftemp = new Buff(2, this, other);
                                bufftemp.stun = true;
                                other.buffs.Add(bufftemp);
                                bufftemp = new Buff(3, this, other);
                                bufftemp.dmgToOther = 25;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(13, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 3:
                                DoDmgToOther(damage + 75, other);
                                bufftemp = new Buff(9, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                    case "Bill":
                        switch (selectedSpecial)
                        {
                            case 1:
                                if (!ceFameuxBill)
                                {
                                    ceFameuxBill = true;
                                    bufftemp = new Buff(1, this, other);
                                    bufftemp.cooldown[selectedSpecial - 1] = true;
                                    buffs.Add(bufftemp);
                                }
                                else
                                {
                                    ceFameuxBill = false;
                                    bufftemp = new Buff(1, this, other);
                                    bufftemp.cooldown[selectedSpecial - 1] = true;
                                    buffs.Add(bufftemp);
                                    selectedSpecial = 4;
                                }
                                break;
                            case 2:
                                if (!ceFameuxBill)
                                {
                                    bufftemp = new Buff(2, this, other);
                                    bufftemp.stun = true;
                                    other.buffs.Add(bufftemp);
                                    bufftemp = new Buff(10, this, other);
                                    bufftemp.cooldown[selectedSpecial - 1] = true;
                                    buffs.Add(bufftemp);
                                }
                                else
                                {
                                    bufftemp = new Buff(4, this, other);
                                    bufftemp.bonusDmg = 30;
                                    buffs.Add(bufftemp);
                                    DoDmgToOther(damage, other, true);
                                    bufftemp = new Buff(12, this, other);
                                    bufftemp.cooldown[selectedSpecial - 1] = true;
                                    buffs.Add(bufftemp);
                                    selectedSpecial = 5;
                                }
                                break;
                            case 3:
                                if (!ceFameuxBill)
                                {
                                    bufftemp = new Buff(3, this, other);
                                    bufftemp.defence = 15;
                                    buffs.Add(bufftemp);
                                    bufftemp = new Buff(3, this, other);
                                    bufftemp.bonusDmg = 30;
                                    buffs.Add(bufftemp);
                                    bufftemp = new Buff(14, this, other);
                                    bufftemp.cooldown[selectedSpecial - 1] = true;
                                    buffs.Add(bufftemp);
                                }
                                else
                                {
                                    DoDmgToOther(damage / 3, other, true);
                                    AddHp(damage / 2);
                                    bufftemp = new Buff(10, this, other);
                                    bufftemp.cooldown[selectedSpecial - 1] = true;
                                    buffs.Add(bufftemp);
                                    selectedSpecial = 6;
                                }
                                break;
                        }
                        break;
                    case "Silver":
                        switch (selectedSpecial)
                        {
                            case 1:
                                if (!silvered)
                                {
                                    DoDmgToOther(damage + 20, other, true);
                                    bufftemp = new Buff(3, this, other);
                                    bufftemp.cooldown[selectedSpecial - 1] = true;
                                    buffs.Add(bufftemp);
                                }
                                else
                                {
                                    bufftemp = new Buff(3, this, other);
                                    bufftemp.armorReduce = 20;
                                    other.buffs.Add(bufftemp);
                                    DoDmgToOther(110, other, true);
                                    bufftemp = new Buff(11, this, other);
                                    bufftemp.cooldown[selectedSpecial - 1] = true;
                                    buffs.Add(bufftemp);
                                    selectedSpecial = 4;
                                }
                                break;
                            case 2:
                                if (!silvered)
                                {
                                    bufftemp = new Buff(2, this, other);
                                    bufftemp.block = true;
                                    buffs.Add(bufftemp);
                                    bufftemp = new Buff(2, this, other);
                                    bufftemp.armorReduce = -20;
                                    buffs.Add(bufftemp);
                                    bufftemp = new Buff(10, this, other);
                                    bufftemp.cooldown[selectedSpecial - 1] = true;
                                    buffs.Add(bufftemp);
                                }
                                else
                                {
                                    bufftemp = new Buff(3, this, other);
                                    bufftemp.stun = true;
                                    other.buffs.Add(bufftemp);
                                    bufftemp = new Buff(6, this, other);
                                    bufftemp.dmgToOther = 30;
                                    buffs.Add(bufftemp);
                                    bufftemp = new Buff(20, this, other);
                                    bufftemp.cooldown[selectedSpecial - 1] = true;
                                    buffs.Add(bufftemp);
                                    selectedSpecial = 5;
                                }
                                break;
                            case 3:
                                if (!silvered)
                                {
                                    damage += 30;
                                    armor += 10;
                                    AddHp(250);
                                    silvered = true;
                                }
                                else
                                {
                                    DoDmgToOther(300, other, true);
                                    bufftemp = new Buff(2, this, other);
                                    bufftemp.block = true;
                                    buffs.Add(bufftemp);
                                    bufftemp = new Buff(12, this, other);
                                    bufftemp.cooldown[selectedSpecial - 1] = true;
                                    buffs.Add(bufftemp);
                                    selectedSpecial = 6;
                                }
                                break;
                        }
                        break;
                    case "Eilli":
                        switch (selectedSpecial)
                        {
                            case 1:
                                healBack = true;
                                bufftemp = new Buff(7, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 2:
                                bufftemp = new Buff(4, this, other);
                                bufftemp.defence = 40;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(12, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 3:
                                DoDmgToOther(40, other);
                                bufftemp = new Buff(3, this, other);
                                bufftemp.stun = true;
                                other.buffs.Add(bufftemp);
                                bufftemp = new Buff(15, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                    case "Beltrame":
                        switch (selectedSpecial)
                        {
                            case 1:
                                bufftemp = new Buff(2, this, other);
                                bufftemp.armorReduce = -20;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(7, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 2:
                                bufftemp = new Buff(2, this, other);
                                bufftemp.stun = true;
                                other.buffs.Add(bufftemp);
                                bufftemp = new Buff(3, this, other);
                                bufftemp.bonusDmg = 20;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(14, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 3:
                                bufftemp = new Buff(3, this, other);
                                bufftemp.stun = true;
                                other.buffs.Add(bufftemp);
                                bufftemp = new Buff(3, this, other);
                                bufftemp.dmgToOther = 40;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(16, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                    case "Akuri":
                        switch (selectedSpecial)
                        {
                            case 1:
                                AddHp((int)GetArmor() * 2);
                                bufftemp = new Buff(7, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 2:
                                DoDmgToOther(50, other, true);
                                bufftemp = new Buff(2, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 3:
                                bufftemp = new Buff(3, this, other);
                                bufftemp.armorReduce = -20;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(6, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                    case "Leeroy Jenkins":
                        switch (selectedSpecial)
                        {
                            case 1:
                                DoDmgToOther(damage * 3, other);
                                bufftemp = new Buff(7, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 2:
                                bufftemp = new Buff(5, this, other);
                                bufftemp.stun = true;
                                other.buffs.Add(bufftemp);
                                bufftemp = new Buff(30, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 3:
                                armor = 0;
                                GetDmg(40);
                                damage += 70;
                                bufftemp = new Buff(1, this, other);
                                bufftemp.cooldown[selectedSpecial - 1] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                }
                texte = name + " utilise: " + Program.Spells[classe][selectedSpecial - 1];
                return true;
            }
            else
            {
                texte = "";
                return false;
            }
        }

        public int CheckCD(int selected)
        {
            foreach (Buff buff in buffs)
            {
                if (buff.cooldown[selected - 1] && buff.activate)
                    return buff.turn;
            }
            if (selected == 0)
                return 100;
            return 0;
        }

        public void newTurn()
        {
            foreach (Buff buff in buffs)
            {
                if (buff.activate && buff.NewTurn())
                    buff.activate = false;
            }
        }
        public bool GetBlock()
        {
            foreach (Buff buff in buffs)
            {
                if (buff.block && buff.activate)
                    return true;
            }
            return false;
        }
        public void Block(charactersActionValue other)
        {
            Buff bufftemp = new Buff(1, this, other);
            bufftemp.defence = 80;
            buffs.Add(bufftemp);
        }

        public int GetDefence()
        {
            int deftemp = defence;
            foreach (Buff buff in buffs)
            {
                if (buff.activate)
                    deftemp -= buff.defence;
            }
            return deftemp;
        }

        public bool IsStunned()
        {
            foreach (Buff buff in buffs)
            {
                if (buff.stun && buff.activate)
                    return true;
            }
            return false;
        }

        public bool Glasses()
        {
            foreach (Buff buff in buffs)
            {
                if (buff.ceFameuxBill && buff.activate)
                    return true;
            }
            return false;
        }
    }

    class Buff
    {
        public int turn = 0, bonusDmg = 0, dmgToOther = 0, armorReduce = 0, defence = 0;
        public bool ignoreDef = false, block = false, activate = true, stun = false, ceFameuxBill = false;
        public List<bool> cooldown = new List<bool> { false, false, false };
        public charactersActionValue ally, enemy;
        public Buff(int _turn, charactersActionValue _ally, charactersActionValue _enemy = null)
        {
            ally = _ally;
            enemy = _enemy;
            turn = _turn;
        }
        public bool NewTurn()
        {
            if (dmgToOther > 0)
                enemy.GetDmg(dmgToOther, ignoreDef);
            turn--;
            return turn == 0;
        }
    }

    class Program
    {
        static List<string> Classe = new List<string>
        {
            "Bestraf",
            "Kolyma",
            "Zelote",
            "Xyns",
            "Daicy",
            "Bill",
            "Silver",
            "Eilli",
            "Beltrame",
            "Akuri",
            "Leeroy Jenkins",
            "Eilli",
        };
        public static Dictionary<string, List<string>> Spells = new Dictionary<string, List<string>>
        {
            {"Bestraf", new List<string>{ "Parade des vents", "Lame Ardente", "Ruse" } },
            {"Kolyma", new List<string>{ "Soin Immédiat", "Pacification", "Jardin Hostile" } },
            {"Zelote", new List<string>{ "Jet du Bouclier", "Blocage" ,"Destruction des défenses"} },
            {"Xyns", new List<string>{ "Espadon de Givre", "Aiguille aqueuse", "Bulle d’eau" } },
            {"Daicy", new List<string>{ "Invisibilité", "Frelon de Flamme", "Lance Incandescente" } },
            {"Bill", new List<string>{ "Lentille Mécanique", "Endormissement", "Sérénité", "Lentille Mécanique", "Fureur du Commandant", "Donnant Donnant" } },
            {"Silver", new List<string>{ "Neutralisation", "L'Eclaire d'argent", "Ange du Tonnerre", "L'Abattement de la Foudre", "Cage de Foudre", "Fierté du Conquérant" } },
            {"Eilli", new List<string>{ "Nocturne de l'apaisement", "Protection Luminescente", "Requiem des plantes" } },
            {"Beltrame", new List<string>{ "Aile de Glace", "Chaîne des Ombre", "Orgue Primordial" } },
            {"Akuri", new List<string>{ "Etoile Central", "Pluie d'astéroïdes", "Barrière de Sel" } },
            {"Leeroy Jenkins", new List<string>{ "LEEROY JENKINS!!!", "LEEROY JENKINS!!!!!!", "LEEROY JENKINS!!!!!!!!" } },
        };
        static void Main()
        {
            GameManager();
            //TestStats();
        }
        static void TestStats()
        {
            int victoryCount1 = 0, virctoryCount2 = 0;
            for (int i = 0; i < 1000; i++)
            {
                charactersActionValue AI1Character = new charactersActionValue(Classes(1), "IA1");
                charactersActionValue AI2Character = new charactersActionValue(Classes(3), "IA2");
                int nombreTour = 0;
                while (true)
                {
                    nombreTour++;

                    if (nombreTour == 100)
                    {
                        AI1Character.Kill();
                        AI2Character.Kill();
                    }

                    if (AI1Character.GetHp() <= 0 && AI2Character.GetHp() <= 0)
                    {
                        Console.WriteLine("Les Nullos on tout les deux Perdu");
                        break;
                    }

                    else if (AI1Character.GetHp() <= 0)
                    {
                        Console.WriteLine("Perdu, meilleurs Chance une prochaine fois");
                        victoryCount1++;
                        break;
                    }

                    else if (AI2Character.GetHp() <= 0)
                    {
                        Console.WriteLine("GG, Tu as gagnÃ©");
                        virctoryCount2++;
                        break;
                    }
                    bool playerReturn = false;
                    int iaspell = 0;
                    while (!playerReturn)
                    {
                        iaspell = IA(4);
                        if (iaspell == 3 && AI1Character.CheckCD(1) > 0 && AI1Character.CheckCD(2) > 0 && AI1Character.CheckCD(3) > 0)
                        {
                            playerReturn = false;
                        }
                        else
                        {
                            playerReturn = true;
                        }
                    }
                    bool playerReturn2 = false;
                    int iaspell2 = 0;
                    while (!playerReturn2)
                    {
                        iaspell2 = IA(4);
                        if (iaspell2 == 3 && AI2Character.CheckCD(1) > 0 && AI2Character.CheckCD(2) > 0 && AI2Character.CheckCD(3) > 0)
                        {
                            playerReturn2 = false;
                        }
                        else
                        {
                            playerReturn2 = true;
                        }
                    }
                    Console.WriteLine(iaspell);
                    Priorite(AI1Character, AI2Character, iaspell, iaspell2);
                    DisplayHealth("", AI2Character, AI1Character);
                    AI1Character.newTurn();
                    AI2Character.newTurn();
                }
            }
            Console.WriteLine("btm:" + victoryCount1 + "/ top:" + virctoryCount2);
        }
        static void GameManager()
        {
            //je vais faire des tour , voila

            bool end = false;
            while (end == false)
            {

                DisplayIntro();

                int playerChar;
                bool finDeParti = false;
                int nombreTour = 0;

                playerChar = DisplayClass();

                charactersActionValue playerCharacter = new charactersActionValue(Classes(playerChar), "Joueur");

                int verdict = IA(Classe.Count() + 1);

                charactersActionValue IACharacter = new charactersActionValue(Classes(verdict), "IA");


                while (finDeParti == false)
                {

                    nombreTour++;

                    if (nombreTour == 100)
                    {
                        playerCharacter.Kill();
                        IACharacter.Kill();
                    }

                    if (playerCharacter.GetHp() <= 0 && IACharacter.GetHp() <= 0)
                    {
                        Console.WriteLine("Les Nullos on tout les deux Perdu");
                        break;
                    }

                    else if (playerCharacter.GetHp() <= 0)
                    {
                        Console.WriteLine("Perdu, meilleurs Chance une prochaine fois");
                        break;
                    }

                    else if (IACharacter.GetHp() <= 0)
                    {
                        Console.WriteLine("GG, Tu as gagnÃ©");
                        break;
                    }

                    DisplayManche(nombreTour);

                    DisplayHealth(playerCharacter.GetClass(), playerCharacter, IACharacter);

                    int playermoov;

                    if (playerCharacter.IsStunned() == true) playermoov = 0;
                    else playermoov = DisplayTurnChoice(playerCharacter);

                    Console.WriteLine();
                    int iaspell;
                    if (IACharacter.IsStunned() == true)
                    {
                        iaspell = 0;
                    }
                    else
                    {
                        iaspell = IA(4);
                        bool playerReturn = false;

                        while (!playerReturn)
                        {
                            iaspell = IA(4);
                            if (iaspell == 3 && IACharacter.CheckCD(1) > 0 && IACharacter.CheckCD(2) > 0 && IACharacter.CheckCD(3) > 0)
                            {
                                playerReturn = false;
                            }
                            else
                            {
                                playerReturn = true;
                            }
                        }
                    }


                    Priorite(playerCharacter, IACharacter, playermoov, iaspell);

                    playerCharacter.newTurn();
                    IACharacter.newTurn();

                    //Affichage encore du choix du joueur et de L'IA

                    //Affichage des choix 
                    //DisplayTurnResult(classeJ)


                    //Application des heal 
                    //Application des dÃ©gat
                }

                string rejouer = "n";
                rejouer = DisplayRePlay();
                if (rejouer == "n")
                {
                    end = true;
                    Console.WriteLine("Barrez vous");
                }
                else
                {
                    Console.WriteLine("C'est reparti");
                }

            }
        }
        static void Priorite(charactersActionValue JValue, charactersActionValue IaValue, int cj, int cia)
        {
            string actJ = "";
            string actIA = "";

            if (cj == 2 || cia == 2)
            {
                if (cj == 2)
                {
                    Console.WriteLine();
                    JValue.Block(IaValue);
                    actJ = "Le Joueur ce défend.";
                }
                if (cia == 2)
                {
                    Console.WriteLine();
                    IaValue.Block(JValue);
                    actIA = "L'Ia ce défend.";
                }
            }

            if (cj == 1 || cia == 1)
            {

                if (cj == 1)
                {
                    Console.WriteLine();
                    JValue.SimpleAttack(IaValue);
                    actJ = "Le Joueur utilise une simple attaque.";
                }
                if (cia == 1)
                {
                    Console.WriteLine();
                    IaValue.SimpleAttack(JValue);
                    actIA = "L'Ia utilise une simple attaque.";
                }
            }
            if (cj == 3 || cia == 3)
            {
                if (cj == 3)
                {
                    actJ = DisplayChoixSpe(JValue, IaValue);

                    //var ran = new Random();
                    //int ranAtSp = ran.Next(1, 4);

                    //bool playerReturn = false;
                    //while (!playerReturn || JValue.CheckCD(ranAtSp) > 0)
                    //{
                    //    ranAtSp = IA(4);
                    //    if (JValue.CheckCD(ranAtSp) == 0)
                    //    {
                    //        playerReturn = true;
                    //    }
                    //}

                    //JValue.DoSpecial(JValue, ranAtSp);
                    //Console.WriteLine("le joueur n°" + ranAtSp);

                }
                if (cia == 3)
                {
                    var ran = new Random();
                    int ranAtSp = ran.Next(1, 4);

                    bool playerReturn = false;
                    while (!playerReturn || IaValue.CheckCD(ranAtSp) > 0)
                    {
                        ranAtSp = IA(4);
                        if (IaValue.CheckCD(ranAtSp) == 0)
                        {
                            playerReturn = true;
                        }
                    }

                    IaValue.DoSpecial(JValue, ranAtSp, out actIA);
                }
            }
            if (cj == 0 || cia == 0)
            {

                if (cj == 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Le Joueur a été assomé pour ce tours");
                }
                if (cia == 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("L'Ia a été assomé pour ce tours");
                }
            }

            Console.WriteLine(actJ);
            Console.WriteLine(actIA);
        }


        static int IA(int maxRandom)
        {
            var ran = new Random();
            int choixBot = ran.Next(1, maxRandom);
            return choixBot;
        }

        static void IATurn(charactersActionValue player, charactersActionValue IAplayer)
        {
            string act;
            int iamoov;
            iamoov = IA(4);

            //attack
            if (iamoov == 1)
            {
                IAplayer.SimpleAttack(player);
                Console.WriteLine("L'IA a décidé d'attaquer !");
            }

            //block
            if (iamoov == 2)
            {
                IAplayer.Block(player);
                Console.WriteLine("L'IA a décidé de bloquer !");
            }

            //attack spé
            if (iamoov == 3)
            {
                var ran = new Random();
                int ranAtSp = ran.Next(1, 4);
                IAplayer.DoSpecial(player, ranAtSp, out act);

                Console.WriteLine("L'IA a décidé de faire l'attaque spéciale n°" + ranAtSp);
            }
        }


        static Tuple<string, int, int, int> Classes(int classeID)
        {
            classeID--;
            List<int> pvList = new List<int>
            {
                1200,
                950,
                1850,
                1280,
                1050,
                1300,
                850,
                1100,
                1160,
                1200,
                1111
            };
            List<int> attaqueList = new List<int>
            {
                95,
                40,
                35,
                70,
                85,
                50,
                45,
                40,
                55,
                37,
                195
            };
            List<int> armorList = new List<int>
            {
                20,
                17,
                55,
                25,
                23,
                13,
                30,
                45,
                37,
                34,
                15
            };
            return new Tuple<string, int, int, int>(Classe[classeID], pvList[classeID], attaqueList[classeID], armorList[classeID]);
        }

        //Partie Affichage


        //Partie Affichage
        //Fct qui affiche l'intro du jeu et msg d'acceuille
        static void DisplayIntro()
        {
            //Affichage logo en ASCII ART
            Console.WriteLine("+------------------------+");
            Console.WriteLine("| Bienvenue dans l'arène |");
            Console.WriteLine("+------------------------+");
        }
        //Fct qui demande au joueur sa classe et la return en int
        static int DisplayClass()
        {
            bool playerReturn = false;
            int playerRead = 0;
            while (!playerReturn)
            {
                Console.WriteLine("Veuillez choisir votre classe de personnage :");
                Console.WriteLine("1 - Bestraf");
                Console.WriteLine("2 - Kolyma");
                Console.WriteLine("3 - Zelote");
                Console.WriteLine("4 - Xyns");
                Console.WriteLine("5 - Daicy");
                Console.WriteLine("6 - Bill");
                Console.WriteLine("7 - Silver");
                Console.WriteLine("8 - Eilli");
                Console.WriteLine("9 - Beltrame");
                Console.WriteLine("10- Akuri");
                Console.WriteLine("11- Leeroy Jenkins");
                Console.WriteLine("12- Silver");
                playerRead = Int32.Parse(Console.ReadLine());
                if (playerRead > 0 && playerRead <= Classe.Count)
                {
                    playerReturn = true;

                }
            }
            return playerRead;
        }
        //Fct qui affiche la manche avec son chiffre
        static void DisplayManche(int index)
        {
            string lenght = new String('-', index % 10);
            Console.WriteLine("+---------" + lenght + "+");
            Console.WriteLine("| Manche " + index + " |");
            Console.WriteLine("+---------" + lenght + "+");
        }
        //Fct qui affiche les PV actuelle du joueur et de L'IA
        static void DisplayHealth(string playerName, charactersActionValue pValue, charactersActionValue iaValue)
        {
            Console.WriteLine("[" + playerName + "][" + pValue.GetClass() + "] HP : " + pValue.GetHp().ToString());
            Console.WriteLine("[IA][" + iaValue.GetClass() + "] HP : " + iaValue.GetHp().ToString());
        }
        //Fct qui demande au joueur son action et la return en int
        static int DisplayTurnChoice(charactersActionValue Value)
        {
            int playerRead = 0;
            bool playerReturn = false;
            while (!playerReturn)
            {
                Console.WriteLine("Actions possibles :");
                Console.WriteLine("1 - Attaquer");
                Console.WriteLine("2 - Défendre");
                Console.WriteLine("3 - Action spéciale");
                Console.WriteLine("Choix :");
                playerRead = Int32.Parse(Console.ReadLine());
                if (playerRead > 0 && playerRead <= 3)
                {
                    if (playerRead == 3 && Value.CheckCD(1) > 0 && Value.CheckCD(2) > 0 && Value.CheckCD(3) > 0)
                    {
                        playerReturn = false;
                    }
                    else
                    {
                        playerReturn = true;
                    }

                }
            }
            return playerRead;
        }
        //Fct qui affiche le resultat des actions choisit
        static string DisplayRePlay()
        {
            string cont = "";
            Console.WriteLine($"Vous voulez rejoué ?");
            Console.Write("Continuez (o/n) : ");
            cont = Console.ReadLine();

            while (cont != "o" && cont != "n")
            {
                Console.WriteLine("EntrÃ© impossible");
                Console.Write("Continuez (o/n) : ");
                cont = Console.ReadLine();
                if (cont == "o" || cont == "n")
                {
                    break;
                }
            }
            if (cont == "n")
            {
                Console.WriteLine();
                return cont;
            }
            return cont;
        }

        static string DisplayChoixSpe(charactersActionValue Value, charactersActionValue Ennemi)
        {
            string act = "";
            int playerRead = 0;
            bool playerReturn = false;
            while (!playerReturn || Value.CheckCD(playerRead) > 0)
            {
                Console.WriteLine("Choix Attaque Spécial :");
                if (Value.silvered == true)
                {
                    Console.WriteLine($"1 - {Spells[Value.GetClass()][4]} (CD : {Value.CheckCD(1)})");
                    Console.WriteLine($"2 - {Spells[Value.GetClass()][5]} (CD : {Value.CheckCD(2)})");
                    Console.WriteLine($"3 - {Spells[Value.GetClass()][6]} (CD : {Value.CheckCD(3)})");

                }
                else
                {
                    Console.WriteLine($"1 - {Spells[Value.GetClass()][0]} (CD : {Value.CheckCD(1)})");
                    if (Value.ceFameuxBill == true)
                    {
                        Console.WriteLine($"2 - {Spells[Value.GetClass()][4]} (CD : {Value.CheckCD(2)})");
                        Console.WriteLine($"3 - {Spells[Value.GetClass()][5]} (CD : {Value.CheckCD(3)})");
                    }
                    else
                    {
                        Console.WriteLine($"2 - {Spells[Value.GetClass()][1]} (CD : {Value.CheckCD(2)})");
                        Console.WriteLine($"3 - {Spells[Value.GetClass()][2]} (CD : {Value.CheckCD(3)})");
                    }
                }
                Console.WriteLine("Choix :");
                playerRead = Int32.Parse(Console.ReadLine());
                if (playerRead > 0 && playerRead <= 3)
                {
                    if (Value.DoSpecial(Ennemi, playerRead, out act) == true)
                    {
                        playerReturn = true;
                        return act;
                    }
                }
            }
            return act;
        }
    }
}
