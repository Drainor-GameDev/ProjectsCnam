using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace JeuDeCombat
{
    class Program
    {
        static Type Classe
        {
                "Damager",
                "Healer",
                "Tank"
        }
        static void GameManager()
        {
            //je vais faire des tour , voila

            bool end = false;

            while(end == false)
            {
                //ici on ferra le choix du personnage avant de commencer

                bool finDeParti = false;

                while(finDeParti,)
            }

        }

        static void IA()
        {
            
        }

        static void Action(int actionID)
        {

        }

        static void Classes(int classeID, out string classe, out int pv, out int attaque)
        {
            classeID++;
            List<string> Classe = new List<string>
            {
                "Damager",
                "Healer",
                "Tank"
            };
            List<int> pvList = new List<int>
            {
                3,
                4,
                5
            };
            List<int> attaqueList = new List<int>
            {
                2,
                1,
                1
            };
            classe = Classe[classeID];
            pv = pvList[classeID];
            attaque = attaqueList[classeID];
        }

        //Partie Affichage

        
        //Fct qui demande au joueur sa classe et la return en int
        static int DisplayClass()
        {
            bool playerReturn = false;
            while (!playerReturn)
            {
                Console.WriteLine("Veuillez choisir votre classe de personnage :");
                Console.WriteLine("1 - Damager");
                Console.WriteLine("2 - Healer");
                Console.WriteLine("3 - Tank");
                string playerRead= "";
                playerRead = Console.ReadLine();
                if (playerRead > 0 && playerRead <= Classe.Count)
                {
                    playerReturn = true;
                    return int32.Parse(playerRead);
                }
            }
        }
        //Fct qui demande au joueur son action et la return en int
        static int DisplayTurnChoice()
        {
            bool playerReturn = false;
            while (!playerReturn)
            {
                Console.WriteLine("Actions possibles :");
                Console.WriteLine("1 - Attaquer");
                Console.WriteLine("2 - Défendre");
                Console.WriteLine("3 - Action spéciale");
                Console.WriteLine("Choix :");
                string playerRead = "";
                playerRead = Console.ReadLine();
                if (playerRead > 0 && playerRead <= 3)
                {
                    playerReturn = true;
                    return int32.Parse(playerRead);
                }
            }
        }
        //Fct qui affiche la manche avec son chiffre
        static void DisplayManche(int index)
        {
            Console.WriteLine("+---------" + "-" * (index % 10) + "+");
            Console.WriteLine("| Manche " + index + " |");
            Console.WriteLine("+---------" + "-" * (index % 10) + "+");
        }
    }
}
