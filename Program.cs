using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading;
using static System.Console;

namespace JeuDeCombat
{
    public class charactersActionValue
    {
        public string classe, name;
        public int damage;
        public int defence;
        public int armor;
        public int hp, maxHp;
        public bool sendBack = false, healBack = false, silvered = false;
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
            if (classe == "Jamii")
            {
                Buff bufftemp = new Buff(13, this);
                bufftemp.cooldown[0] = true;
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
                    try { hp = hp - Math.Clamp(dmg - GetDefence(), 0, dmg); } catch { hp -= dmg; }
                }
                else
                {
                    if (!GetBlock())
                        try { hp = hp - Math.Clamp((int)(dmg * (1f - (GetArmor() / 100f))) + GetDefence(), 0, dmg); } catch { hp -= dmg; }
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

        public bool DoSpecial(charactersActionValue other, int selectedSpecial)
        {
            //selectedSpecial--;
            if (CheckCD(selectedSpecial) == 0)
            {
                Buff bufftemp;
                switch (classe)
                {
                    case "Bestraf":
                        switch (selectedSpecial)
                        {
                            case 0:
                                sendBack = true;
                                bufftemp = new Buff(8, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 1:
                                bufftemp = new Buff(3, this, other);
                                bufftemp.bonusDmg = 50;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(12, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 2:
                                DoDmgToOther(damage, other, true);
                                bufftemp = new Buff(6, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                    case "Kolyma":
                        switch (selectedSpecial)
                        {
                            case 0:
                                AddHp(250);
                                bufftemp = new Buff(14, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 1:
                                bufftemp = new Buff(5, this, other);
                                bufftemp.bonusDmg = -30;
                                other.buffs.Add(bufftemp);
                                bufftemp = new Buff(12, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 2:
                                bufftemp = new Buff(4, this, other);
                                bufftemp.dmgToOther = 20;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(6, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                    case "Zelote":
                        switch (selectedSpecial)
                        {
                            case 0:
                                bufftemp = new Buff(4, this, other);
                                bufftemp.bonusDmg = 15;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(12, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                DoDmgToOther(damage, other);
                                GetDmg(150);
                                break;
                            case 1:
                                bufftemp = new Buff(2, this, other);
                                bufftemp.block = true;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(7, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 2:
                                bufftemp = new Buff(3, this, other);
                                bufftemp.armorReduce = 50;
                                other.buffs.Add(bufftemp);
                                bufftemp = new Buff(10, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                    case "Xyns":
                        switch (selectedSpecial)
                        {
                            case 0:
                                bufftemp = new Buff(2, this, other);
                                bufftemp.bonusDmg = damage;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(7, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 1:
                                bufftemp = new Buff(4, this, other);
                                bufftemp.defence = 30;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(9, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 2:
                                bufftemp = new Buff(2, this, other);
                                bufftemp.stun = true;
                                other.buffs.Add(bufftemp);
                                bufftemp = new Buff(15, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                    case "Daicy":
                        switch (selectedSpecial)
                        {
                            case 0:
                                bufftemp = new Buff(3, this, other);
                                bufftemp.block = true;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(10, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 1:
                                bufftemp = new Buff(2, this, other);
                                bufftemp.stun = true;
                                other.buffs.Add(bufftemp);
                                bufftemp = new Buff(3, this, other);
                                bufftemp.dmgToOther = 25;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(13, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 2:
                                DoDmgToOther(damage + 75, other);
                                bufftemp = new Buff(9, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                    case "Bill":
                        switch (selectedSpecial)
                        {
                            case 0:
                                if (!silvered)
                                {
                                    silvered = true;
                                    bufftemp = new Buff(1, this, other);
                                    bufftemp.cooldown[selectedSpecial] = true;
                                    buffs.Add(bufftemp);
                                }
                                else
                                {
                                    silvered = false;
                                    bufftemp = new Buff(1, this, other);
                                    bufftemp.cooldown[selectedSpecial] = true;
                                    buffs.Add(bufftemp);
                                    selectedSpecial = 4;
                                }
                                break;
                            case 1:
                                if (!silvered)
                                {
                                    bufftemp = new Buff(2, this, other);
                                    bufftemp.stun = true;
                                    other.buffs.Add(bufftemp);
                                    bufftemp = new Buff(10, this, other);
                                    bufftemp.cooldown[selectedSpecial] = true;
                                    buffs.Add(bufftemp);
                                }
                                else
                                {
                                    bufftemp = new Buff(4, this, other);
                                    bufftemp.bonusDmg = 30;
                                    buffs.Add(bufftemp);
                                    DoDmgToOther(damage, other, true);
                                    bufftemp = new Buff(12, this, other);
                                    bufftemp.cooldown[selectedSpecial] = true;
                                    buffs.Add(bufftemp);
                                    selectedSpecial = 5;
                                }
                                break;
                            case 2:
                                if (!silvered)
                                {
                                    bufftemp = new Buff(3, this, other);
                                    bufftemp.defence = 15;
                                    buffs.Add(bufftemp);
                                    bufftemp = new Buff(3, this, other);
                                    bufftemp.bonusDmg = 30;
                                    buffs.Add(bufftemp);
                                    bufftemp = new Buff(14, this, other);
                                    bufftemp.cooldown[selectedSpecial] = true;
                                    buffs.Add(bufftemp);
                                }
                                else
                                {
                                    DoDmgToOther(damage / 3, other, true);
                                    AddHp(damage / 2);
                                    bufftemp = new Buff(10, this, other);
                                    bufftemp.cooldown[selectedSpecial] = true;
                                    buffs.Add(bufftemp);
                                    selectedSpecial = 6;
                                }
                                break;
                        }
                        break;
                    case "Silver":
                        switch (selectedSpecial)
                        {
                            case 0:
                                if (!silvered)
                                {
                                    DoDmgToOther(damage + 20, other, true);
                                    bufftemp = new Buff(3, this, other);
                                    bufftemp.cooldown[selectedSpecial] = true;
                                    buffs.Add(bufftemp);
                                }
                                else
                                {
                                    bufftemp = new Buff(3, this, other);
                                    bufftemp.armorReduce = 20;
                                    other.buffs.Add(bufftemp);
                                    DoDmgToOther(110, other, true);
                                    bufftemp = new Buff(11, this, other);
                                    bufftemp.cooldown[selectedSpecial] = true;
                                    buffs.Add(bufftemp);
                                    selectedSpecial = 4;
                                }
                                break;
                            case 1:
                                if (!silvered)
                                {
                                    bufftemp = new Buff(2, this, other);
                                    bufftemp.block = true;
                                    buffs.Add(bufftemp);
                                    bufftemp = new Buff(2, this, other);
                                    bufftemp.armorReduce = -20;
                                    buffs.Add(bufftemp);
                                    bufftemp = new Buff(10, this, other);
                                    bufftemp.cooldown[selectedSpecial] = true;
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
                                    bufftemp.cooldown[selectedSpecial] = true;
                                    buffs.Add(bufftemp);
                                    selectedSpecial = 5;
                                }
                                break;
                            case 2:
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
                                    bufftemp.cooldown[selectedSpecial] = true;
                                    buffs.Add(bufftemp);
                                    selectedSpecial = 6;
                                }
                                break;
                        }
                        break;
                    case "Eilli":
                        switch (selectedSpecial)
                        {
                            case 0:
                                healBack = true;
                                bufftemp = new Buff(14, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 1:
                                bufftemp = new Buff(4, this, other);
                                bufftemp.defence = 40;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(12, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 2:
                                DoDmgToOther(40, other);
                                bufftemp = new Buff(3, this, other);
                                bufftemp.stun = true;
                                other.buffs.Add(bufftemp);
                                bufftemp = new Buff(15, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                    case "Beltrame":
                        switch (selectedSpecial)
                        {
                            case 0:
                                bufftemp = new Buff(2, this, other);
                                bufftemp.armorReduce = -20;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(7, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 1:
                                bufftemp = new Buff(2, this, other);
                                bufftemp.stun = true;
                                other.buffs.Add(bufftemp);
                                bufftemp = new Buff(3, this, other);
                                bufftemp.bonusDmg = 20;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(14, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 2:
                                bufftemp = new Buff(3, this, other);
                                bufftemp.stun = true;
                                other.buffs.Add(bufftemp);
                                bufftemp = new Buff(3, this, other);
                                bufftemp.dmgToOther = 40;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(16, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                    case "Akuri":
                        switch (selectedSpecial)
                        {
                            case 0:
                                AddHp((int)GetArmor() * 2);
                                bufftemp = new Buff(7, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 1:
                                DoDmgToOther(50, other, true);
                                bufftemp = new Buff(2, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 2:
                                bufftemp = new Buff(3, this, other);
                                bufftemp.armorReduce = -20;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(6, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                    case "Leeroy Jenkins":
                        switch (selectedSpecial)
                        {
                            case 0:
                                DoDmgToOther(damage * 3, other);
                                bufftemp = new Buff(7, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 1:
                                bufftemp = new Buff(5, this, other);
                                bufftemp.stun = true;
                                other.buffs.Add(bufftemp);
                                bufftemp = new Buff(30, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 2:
                                armor = 0;
                                GetDmg(40);
                                damage += 70;
                                bufftemp = new Buff(1, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                    case "Jamii":
                        switch (selectedSpecial)
                        {
                            case 0:
                                other.hp = 1;
                                bufftemp = new Buff(2, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 1:
                                sendBack = true;
                                DoDmgToOther(200, other);
                                bufftemp = new Buff(13, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 2:
                                healBack = true;
                                bufftemp = new Buff(2, this, other);
                                bufftemp.armorReduce = 20;
                                other.buffs.Add(bufftemp);
                                bufftemp = new Buff(6, this, other);
                                bufftemp.cooldown[selectedSpecial] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                }
                return true;
            }
            else
            {
                string texte = "";
                return false;
            }
        }

        public int CheckCD(int selected)
        {
            foreach (Buff buff in buffs)
            {
                if (buff.cooldown[selected] && buff.activate)
                    return buff.turn;
            }
            if (selected < 0)
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
    public class Menu
    {
        public string prompt;
        public List<string> options = new List<string>();
        public int index = 0;
        public DisplayManager manager;
        bool isCenter;
        public Menu(string prompt, List<string> options, DisplayManager manager, bool isCenter)
        {
            this.prompt = prompt;
            this.options = options;
            this.index = 0;
            this.manager = manager;
            this.isCenter = isCenter;
        }
        public void DisplayOption(int xPos, int yPos, int _index)
        {
            string[] optionDisplay = new string[options.Count];
            manager.DisplayOnScreen(xPos,yPos - 2,prompt,isCenter);
            for (int i = 0; i < options.Count; i++)
            {
                if (i == _index)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                optionDisplay[i] = options[i];
                manager.DisplayOnScreen(xPos, yPos + i, optionDisplay[i], isCenter);
                Console.ResetColor();
            }
        }
        public void Run(int xPos, int yPos, ref int _index)
        {
            ConsoleKey keyPressed;
            do
            {
                manager.Display();
                DisplayOption(xPos, yPos, _index);
                ConsoleKeyInfo keyInfo = ReadKey();
                keyPressed = keyInfo.Key;
                if (keyPressed == ConsoleKey.UpArrow)
                {
                    _index--;
                    //Changement Bouton
                }
                if (keyPressed == ConsoleKey.DownArrow)
                {
                    _index++;
                    //Cnahgement Bouton
                }
                if (_index < 0)
                {
                    _index = options.Count - 1;
                }
                _index %= options.Count;
            }
            while (keyPressed != ConsoleKey.Enter && keyPressed != ConsoleKey.Spacebar);
            //Selection Bouton
        }
    }

    public class ClasseData
    {
        DisplayManager manager;
        string nom;
        int health;
        int attaque;
        int armor;
        List<string> attaqueSpe;
        List<string> toDisplay;

        public ClasseData(DisplayManager _manager)
        {
            manager = _manager;
        }
        public void SetData(Tuple<string, int, int, int> _data, List<string> actionSpe)
        {
            nom = _data.Item1;
            health = _data.Item2;
            attaque = _data.Item3;
            armor = _data.Item4;
            //attaqueSpe = _attaqueSpe;

            toDisplay = new List<string>();
            toDisplay.Add("Nom : " + nom);
            toDisplay.Add("Point de Vie : " + health.ToString());
            toDisplay.Add("Attaque : " + attaque.ToString());
            toDisplay.Add("Armure : " + armor.ToString());
            for (int i = 0; i < actionSpe.Count; i++)
            {
            toDisplay.Add("Attaque Spéciale n°"+(i+1)+" : " + actionSpe[i]);

            }
           // toDisplay.Add("Attaque Spéciale n°2 : " + actionSpe[1]);
            //toDisplay.Add("Attaque Spéciale n°3 : " + actionSpe[2]);
        }
        public void DisplayClassData(int xPos, int yPos, Tuple<string, int, int, int> classData, List<string> actionSpe)
        {
            SetData(classData, actionSpe);
            //Beep(4000,2);
            for (int i = 0; i < toDisplay.Count; i++)
            {
                manager.DisplayOnScreen(xPos, yPos + i, toDisplay[i], false);
            }
        }
    }
    public class DisplayManager
    {
        public List<Action> displayAction = new List<Action>();

        public void Display()
        {
            Console.Clear();
            foreach (Action action in displayAction)
            {
                action();
            }
        }
        public void AddDisplay(Action action)
        {
            displayAction.Add(action);
        }
        public void RemoveDisplay(Action action)
        {
            displayAction.Remove(action);
        }
        public void ClearList()
        {
            displayAction.Clear();
        }
        public void DisplayOnScreen(int xPos, int yPos, string toDisplay, bool isCenter)
        {
            Console.SetCursorPosition(xPos - ((isCenter) ? (toDisplay.Length / 2) : 0), yPos);
            Console.Write(toDisplay);
        }
    }
    class Program
    {
        static public DisplayManager manager = new DisplayManager();
        static public ClasseData classeData = new ClasseData(manager);
        static public int index;
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
            "Jamii",
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
            {"Jamii", new List<string>{ "ÉNORME !!!", "Chauffe Marcel !!!", "DIT MOI JAMI!!!" } }
        };
        static void Main()
        {
            DisplayInit();
            GameManager();
            //TestStats();
        }
        /*   static void TestStats()
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
           }*/
        static void GameManager()
        {
            //je vais faire des tour , voila
            #region Affichage
            //Menu de base
            manager.AddDisplay(DisplayIntro);
            manager.Display();
            DisplayGlobaleMenu(ref index);
            bool end = (index == 0) ? false : true;
            while (!end)
            {

                //Menu de selection de classe
                manager.ClearList();
                manager.AddDisplay(DisplayIntro);
                manager.AddDisplay(delegate { classeData.DisplayClassData(Console.WindowWidth / 4 * 3, 20, Classes(index), SetSpellList(index)); });
                manager.Display();
                DisplayClassMenu(ref index, Classe);
                List<string> spells = SetSpellList(index);
                charactersActionValue player = new charactersActionValue(Classes(index), "Joueur");
                charactersActionValue ordi = new charactersActionValue(Classes(IA(Classe.Count())), "Ordinateur");
                //Mise En place des Tours
                //Mise en place de la boucle de jeu
                int nRound = 0;
                manager.ClearList();
                manager.AddDisplay(DisplayIntro);
                manager.AddDisplay(delegate { DisplayManche(nRound); });
                manager.AddDisplay(delegate { DisplayCharacterData(player, true); });
                manager.AddDisplay(delegate { DisplayCharacterData(ordi, false); });
                manager.Display();
                //Boucle Princiaple
                while (!CheckEnd(player, ordi))
                {
                    nRound++;
                    bool choosedAction = false;
                    int playerAction = -1, iaAction = -1;
                    int playerSpeAction = 0,iaSpeAction = 0;
                    //Partie Joueur
                    while (!choosedAction)
                    {
                        index = 0;
                        if (!player.IsStunned())
                            DisplayTurnChoice(ref index);

                        if (index == 2)
                        {
                            index = 0;
                            spells.Clear();
                            for (int i = 0; i < Spells[Classe[index + 1]].Count; i++)
                            {
                                spells.Add((Spells[player.classe][i + (player.silvered ? 3 : 0)]) + " [" + player.CheckCD(i) + "]");
                            }
                            DisplayChoixSpe(ref index, spells);
                            if (index != 3)
                            {
                                playerSpeAction = index;
                                playerAction = 2;
                                if(player.CheckCD(playerSpeAction)==0)
                                    choosedAction = true;
                            }
                        }
                        else
                        {
                            choosedAction = true;
                            playerAction = index;
                        }

                    }
                    if (player.IsStunned())
                    {
                        playerAction = 3;
                    }
                    //Partie Ordi
                    choosedAction = false;

                    if (ordi.IsStunned())
                        iaAction = 3;
                    else
                    {
                        while (!choosedAction)
                        {
                            iaAction = IA(3);
                            choosedAction = (iaAction == 2 && ordi.CheckCD(0) > 0 && ordi.CheckCD(1) > 0 && ordi.CheckCD(2) > 0) ? false : true;
                        }
                        if(iaAction == 2)
                        {
                            do
                            {
                                iaSpeAction = IA(3);
                                //Beep();
                            }
                            while(ordi.CheckCD(iaSpeAction)>0);
                        }
                    }
                    Priorite(player, ordi, playerAction, iaAction,playerSpeAction,iaSpeAction);
                    DisplayResultAction(player.name,true, playerAction, Spells[player.classe][playerSpeAction]);
                    DisplayResultAction(ordi.name, false, iaAction, Spells[ordi.classe][iaSpeAction]);
                    player.newTurn();
                    ordi.newTurn();
                    WaitingKey();
                }
                index = 0;
                manager.ClearList();
                manager.AddDisplay(DisplayIntro);
                manager.Display();
                DisplayWinner(player, ordi);
                WaitingKey();
                manager.Display();
                DisplayReplayMenu(ref index);
                end = index == 0 ? false : true;
            }
            //Fin Boucle de Jeu
            #endregion
        }
        static void Priorite(charactersActionValue player, charactersActionValue ordi, int joueurAction, int iaAction, int joueurSpe = 0,int iaSpe = 0)
        {
            //Se defend
            if (joueurAction == 1)
                player.Block(ordi);
            if (iaAction == 1)
                ordi.Block(player);
            //Attaque
            if (joueurAction == 0)
                player.SimpleAttack(ordi);
            if (iaAction == 0)
                ordi.SimpleAttack(player);
            //DoSpeciale
            if (joueurAction == 2)
                player.DoSpecial(ordi, joueurSpe);
            if (iaAction == 2)
                ordi.DoSpecial(player, iaSpe);
                //Console.WriteLine("[Joueur]" + joueurAction + " " + joueurSpe + "[Ordi]" + iaAction + " " + iaSpe + " CD " + ordi.CheckCD(0) + " " + ordi.CheckCD(1) + " " + ordi.CheckCD(2));
            //Si action <= 0 character Assomé et ne joue pas
        }

        static int IA(int maxRandom)
        {
            var ran = new Random();
            int choixBot = ran.Next(0, maxRandom);
            return choixBot;
        }

        /*  static void IATurn(charactersActionValue player, charactersActionValue IAplayer)
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
          }*/

        static Tuple<string, int, int, int> Classes(int classeID)
        {
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
                1111,
                666
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
                195,
                5
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
                15,
                75
            };
            return new Tuple<string, int, int, int>(Classe[classeID], pvList[classeID], attaqueList[classeID], armorList[classeID]);
        }

        static List<string> SetSpellList(int index)
        {
            List<string> spells = new List<string>();
            for (int i = 0; i < Spells[Classe[index]].Count; i++)
            {
                spells.Add(Spells[Classe[index]][i]);
            }
            return spells;
        }
        #region Affichage
        //Partie Affichage
        static void DisplayInit()
        {
            //Console.SetWindowPosition(1,1);
            Console.SetBufferSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
            Console.SetWindowSize(Console.LargestWindowWidth - 1, Console.LargestWindowHeight - 1);
            Console.Title = ("Jeu de Combat AAA");
            //Console.SetCursorPosition(Console.LargestWindowWidth / 2, 0);
            Console.CursorVisible = false;
            //Console.SetWindowPosition(1,1);
            Console.Clear();
            //Console.Beep();
        }
        //Partie Affichage
        //Fct qui affiche l'intro du jeu et msg d'acceuille
        static void DisplayIntro()
        {
            List<string> menu = new List<string>();
            menu.Add("   .-,  ,---.   .-. .-.     ,'|\"\\    ,---.       ,--,    .---.             ,---.     .--.    _______     .--.     .--.     .--.   \r");
            menu.Add("   | |  | .-'   | | | |     | |\\ \\   | .-'     .' .')   / .-. )  |\\    /|  | .-.\\   / /\\ \\  |__   __|   / /\\ \\   / /\\ \\   / /\\ \\  \r");
            menu.Add("   | |  | `-.   | | | |     | | \\ \\  | `-.     |  |(_)  | | |(_) |(\\  / |  | |-' \\ / /__\\ \\   )| |     / /__\\ \\ / /__\\ \\ / /__\\ \\ \r");
            menu.Add("   | |  | .-'   | | | |     | |  \\ \\ | .-'     \\  \\     | | | |  (_)\\/  |  | |--. \\|  __  |  (_) |     |  __  | |  __  | |  __  | \r");
            menu.Add("(`-' |  |  `--. | `-')|     /(|`-' / |  `--.    \\  `-.  \\ `-' /  | \\  / |  | |`-' /| |  |)|    | |     | |  |)| | |  |)| | |  |)| \r");
            menu.Add(" \\_ )|  /( __.' `---(_)    (__)`--'  /( __.'     \\____\\  )---'   | |\\/| |  /( `--' |_|  (_)    `-'     |_|  (_) |_|  (_) |_|  (_) \r");
            for (int i = 0; i < menu.Count; i++)
            {
                manager.DisplayOnScreen((Console.WindowWidth / 2), i, menu[i], true);
            }
        }
        //Fct qui demande au joueur sa classe et la return en int

        static void DisplayGlobaleMenu(ref int menuIndex)
        {
            List<string> menuName = new List<string> { "JOUER", "QUITTER" };
            Menu mainMenu = new Menu("Bienvenue dans l'arène", menuName, manager, false);
            mainMenu.Run(Console.WindowWidth / 3, 20, ref menuIndex);
        }
        static void DisplayClassMenu(ref int classIndex, List<string> option, string playerName = "")
        {
            Menu mainMenu = new Menu("Choisissez votre classe " + playerName, option, manager, false);
            mainMenu.Run(Console.WindowWidth / 3, 20, ref classIndex);
        }
        static void DisplayReplayMenu(ref int replayIndex)
        {
            List<string> menuName = new List<string> { "REJOUER", "QUITTER" };
            Menu mainMenu = new Menu("Voulez-vous rejouer", menuName, manager, true);
            mainMenu.Run(Console.WindowWidth / 2, Console.WindowHeight / 2, ref replayIndex);
        }
        //Fct qui affiche la manche avec son chiffre
        static void DisplayManche(int index)
        {
            string lenght = new string('-', index.ToString().Length);
            List<string> toDisplay = new List<string>();
            toDisplay.Add("+---------" + lenght + "+");
            toDisplay.Add("| Manche " + index + " |");
            toDisplay.Add("+---------" + lenght + "+");
            for (int i = 0; i < toDisplay.Count; i++)
            {
                manager.DisplayOnScreen((Console.WindowWidth / 2), 7 + i, toDisplay[i], true);
            }
        }

        static void DisplayCharacterData(charactersActionValue characterData, bool isPlayer)
        {
            string lenght = new string('-', 33);
            int hp = characterData.hp;
            int mapHp = characterData.maxHp;
            List<string> toDisplay = new List<string>();
            toDisplay.Add("+" + lenght + "+");
            toDisplay.Add("| " + characterData.name + new String(' ', 31 - (characterData.name.Length + characterData.GetClass().Length)) + characterData.GetClass() + " |");
            toDisplay.Add("| PV : [" + hp + "|" + mapHp + "] " + new string(' ', 9 - (hp.ToString().Length + mapHp.ToString().Length)) + " [" + new string('-', ((hp * 10) / mapHp)) + new string(' ', 10 - ((hp * 10) / mapHp)) + "] |");
            toDisplay.Add("| ATTAQUE : [" + characterData.damage + "]" + new string(' ', 20 - characterData.damage.ToString().Length) + "|");
            toDisplay.Add("| ARMURE : [" + characterData.armor + "]" + new string(' ', 21 - characterData.armor.ToString().Length) + "|");
            toDisplay.Add("+" + lenght + "+");

            for (int i = 0; i < toDisplay.Count; i++)
            {
                manager.DisplayOnScreen(isPlayer ? 0 : Console.WindowWidth - toDisplay[0].Length, 10 + i, toDisplay[i], false);
            }
        }
        //Fct qui demande au joueur son action et la return en int
        static void DisplayTurnChoice(ref int turnIndex)
        {
            List<string> menuName = new List<string> { "ATTAQUER", "DEFENDRE", "ACTION SPECIALE" };
            Menu turnMenu = new Menu("", menuName, manager, true);
            turnMenu.Run(Console.WindowWidth / 2, 11, ref turnIndex);
        }
        static void DisplayChoixSpe(ref int speIndex, List<string> _spellsList)
        {
            List<string> menuName = _spellsList;
            menuName.Add("RETOUR");
            Menu turnMenu = new Menu("", menuName, manager, true);
            turnMenu.Run(Console.WindowWidth / 2, 11, ref speIndex);
        }
        static void DisplayResultAction(string name, bool isPlayer,int action, string actionSpe = "")
        {
            string toDisplay = "[" + name + "] ";
            switch (action)
            {
                case 0:
                    toDisplay += "Choisit d'attaquer l'adversaire";
                    break;
                case 1:
                    toDisplay += "Choisit de se defendre";
                    break;
                case 2:
                    toDisplay += "Choisit d'effectuer l'action spéciale : " + actionSpe;
                    break;
                case 3:
                    toDisplay += "Est incapable d'agir";
                    break;
                default:
                    break;
            }
                manager.DisplayOnScreen(Console.WindowWidth/2, 30 + (!isPlayer?1:0), toDisplay, true);
        }
        static bool CheckEnd(charactersActionValue player,charactersActionValue ordi)
        {
            int playerHp = player.hp, ordiHp = ordi.hp;
            if (playerHp <= 0 || ordiHp <= 0)
                return true;
            else 
                return false;
        }
        static void DisplayWinner(charactersActionValue player,charactersActionValue ordi)
        {
            int playerHp = player.hp, ordiHp = ordi.hp;
            string winnerName = "";
            if (playerHp > 0 && ordiHp <= 0) 
                winnerName = player.name;
            if (ordiHp > 0 && playerHp <= 0)
                winnerName = ordi.name;
            if (winnerName != "")
                manager.DisplayOnScreen(Console.WindowWidth / 2, 30, "Le Gagnant est " + winnerName, true);
            else
                manager.DisplayOnScreen(Console.WindowWidth / 2, 30, "Egalité", true);
        }
        static void WaitingKey()
        {
            do
                manager.DisplayOnScreen(Console.WindowWidth / 2, 40, "Appuyer sur une touche pour continuer",true);
            while (ReadKey(true) == null) ;
        }
        #endregion
    }
}
