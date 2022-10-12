﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading;

namespace JeuDeCombat
{
    public class charactersActionValue
    {
        string classe;
        int damage;
        int defence;
        int armor;
        int hp, maxHp;
        bool sendBack = false;
        List<Buff> buffs = new List<Buff>();

        public charactersActionValue(Tuple<string, int, int, int> _classe)
        {
            classe = _classe.Item1;
            damage = _classe.Item3;
            maxHp = _classe.Item2;
            armor = _classe.Item4;
            hp = maxHp;
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
                if(buff.activate)
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
                if(buff.activate)
                    armortemp -= buff.armorReduce;
            }
            return armortemp;
        }

        public bool DoSpecial(charactersActionValue other, int selectedSpecial)
        {
            //selectedSpecial--;
            if (CheckCD(selectedSpecial) == 0 && selectedSpecial != 0)
            {
                Buff bufftemp;
                switch (classe)
                {
                    case "Damager":
                        switch (selectedSpecial)
                        {
                            case 1:
                                sendBack = true;
                                bufftemp = new Buff(4, this, other);
                                bufftemp.cooldown[selectedSpecial-1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 2:
                                bufftemp = new Buff(3, this, other);
                                bufftemp.bonusDmg = 50;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(7, this, other);
                                bufftemp.cooldown[selectedSpecial-1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 3:
                                DoDmgToOther(damage, other, true);
                                bufftemp = new Buff(3, this, other);
                                bufftemp.cooldown[selectedSpecial-1] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                    case "Healer":
                        switch (selectedSpecial)
                        {
                            case 1:
                                AddHp(250);
                                bufftemp = new Buff(5, this, other);
                                bufftemp.cooldown[selectedSpecial-1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 2:
                                bufftemp = new Buff(5, this, other);
                                bufftemp.bonusDmg = -30;
                                other.buffs.Add(bufftemp);
                                bufftemp = new Buff(12, this, other);
                                bufftemp.cooldown[selectedSpecial-1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 3:
                                bufftemp = new Buff(4, this, other);
                                bufftemp.dmgToOther = 20;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(6, this, other);
                                bufftemp.cooldown[selectedSpecial-1] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                    case "Tank":
                        switch (selectedSpecial)
                        {
                            case 1:
                                bufftemp = new Buff(4, this, other);
                                bufftemp.bonusDmg = 15;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(12, this, other);
                                bufftemp.cooldown[selectedSpecial-1] = true;
                                buffs.Add(bufftemp);
                                DoDmgToOther(damage, other);
                                GetDmg(150);
                                break;
                            case 2:
                                bufftemp = new Buff(2, this, other);
                                bufftemp.block = true;
                                buffs.Add(bufftemp);
                                bufftemp = new Buff(7, this, other);
                                bufftemp.cooldown[selectedSpecial-1] = true;
                                buffs.Add(bufftemp);
                                break;
                            case 3:
                                bufftemp = new Buff(3, this, other);
                                bufftemp.armorReduce = 50;
                                other.buffs.Add(bufftemp);
                                bufftemp = new Buff(10, this, other);
                                bufftemp.cooldown[selectedSpecial-1] = true;
                                buffs.Add(bufftemp);
                                break;
                        }
                        break;
                }
                return true;
            }
            else
            {
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
            Buff bufftemp = new Buff(2, this, other);
            bufftemp.defence = 80;
            buffs.Add(bufftemp);
        }

        public int GetDefence()
        {
            int deftemp = defence;
            foreach (Buff buff in buffs)
            {
                if(buff.activate)
                    deftemp -= buff.defence;
            }
            return deftemp;
        }
    }

    class Buff
    {
        public int turn = 0, bonusDmg = 0, dmgToOther = 0, armorReduce = 0, defence = 0;
        public bool ignoreDef = false, block = false, activate = true;
        public List<bool> cooldown = new List<bool>{false, false, false};
        public charactersActionValue ally, enemy;
        public Buff(int _turn, charactersActionValue _ally, charactersActionValue _enemy)
        {
            ally = _ally;
            enemy = _enemy;
            turn = _turn;
        }
        public bool NewTurn()
        {
            enemy.GetDmg(dmgToOther, ignoreDef);
            turn--;
            return turn == 0;
        }
    }

    class Program
    {
        static List<string> Classe = new List<string>
        {
            "Damager",
            "Healer",
            "Tank",

        };
        static void Main()
        {
            GameManager();
            //TestStats();
        }
        static void TestStats()
        {
            int victoryCount1 = 0, virctoryCount2 = 0;
            for(int i = 0; i < 1000; i++)
            {
                charactersActionValue AI1Character = new charactersActionValue(Classes(1));
                charactersActionValue AI2Character = new charactersActionValue(Classes(3));
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

                charactersActionValue playerCharacter = new charactersActionValue(Classes(playerChar));

                int verdict = IA(Classe.Count() + 1);

                charactersActionValue IACharacter = new charactersActionValue(Classes(verdict));


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
                    playermoov = DisplayTurnChoice(playerCharacter);

                    Console.WriteLine();
                    int iaspell = IA(4);
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


            if (cj == 2 || cia == 2)
            {
                if (cj == 2)
                {
                    Console.WriteLine();
                    JValue.Block(IaValue);
                    Console.WriteLine("Le Joueur ce défend.");
                }
                if (cia == 2)
                {
                    Console.WriteLine();
                    IaValue.Block(JValue);
                    Console.WriteLine("L'Ia ce défend.");
                }
            }

            if (cj == 1 || cia == 1)
            {

                if (cj == 1)
                {
                    Console.WriteLine();
                    JValue.SimpleAttack(IaValue);
                    Console.WriteLine("Le Joueur utilise une simple attaque.");
                }
                if (cia == 1)
                {
                    Console.WriteLine();
                    IaValue.SimpleAttack(JValue);
                    Console.WriteLine("L'Ia utilise une simple attaque.");
                }
            }
            if (cj == 3 || cia == 3)
            {
                if (cj == 3)
                {
                    DisplayChoixSpe(JValue, IaValue);
                    Console.WriteLine();
                    Console.WriteLine("Le Joueur fait une attaque spécial.");

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

                    IaValue.DoSpecial(JValue, ranAtSp);
                    Console.WriteLine("L'IA a décidé de faire l'attaque spéciale n°" + ranAtSp);
                }
            }
        }


        static int IA(int maxRandom)
        {
            var ran = new Random();
            int choixBot = ran.Next(1, maxRandom);
            return choixBot;
        }

        static void IATurn(charactersActionValue player, charactersActionValue IAplayer)
        {
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
                IAplayer.DoSpecial(player, ranAtSp);

                Console.WriteLine("L'IA a décidé de faire l'attaque spéciale n°" + ranAtSp);
            }
        }


        static Tuple<string, int, int, int> Classes(int classeID)
        {
            classeID--;
            List<int> pvList = new List<int>
            {
                240,
                140,
                300
            };
            List<int> attaqueList = new List<int>
            {
                95,
                40,
                35
            };
            List<int> armorList = new List<int>
            {
                20,
                20,
                55
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
                Console.WriteLine("1 - Damager");
                Console.WriteLine("2 - Healer");
                Console.WriteLine("3 - Tank");
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
        static void DisplayChoixSpe(charactersActionValue Value, charactersActionValue Ennemi)
        {
            int playerRead = 0;
            bool playerReturn = false;
            while (!playerReturn || Value.CheckCD(playerRead) > 0)
            {
                Console.WriteLine("Choix Attaque Spécial :");
                Console.WriteLine($"1 - 1er Attaque Spe (CD : {Value.CheckCD(1)})");
                Console.WriteLine($"2 - 2eme Attaque Spe (CD : {Value.CheckCD(2)})");
                Console.WriteLine($"3 - 3eme Attaque Spe (CD : {Value.CheckCD(3)})");
                Console.WriteLine("Choix :");
                playerRead = Int32.Parse(Console.ReadLine());
                if (playerRead > 0 && playerRead <= 3)
                {
                    if (Value.DoSpecial(Ennemi, playerRead) == true)
                    {
                        playerReturn = true;
                        break;
                    }
                }
            }
            return;
        }
    }
}