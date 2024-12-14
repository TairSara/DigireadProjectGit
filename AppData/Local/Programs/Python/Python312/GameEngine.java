/**
 * Aviram Morad 318691615
 * Alon Ilibman 206542763
 */

package game;
import game.competition.Competition;
import game.competition.Competitor;

/**
 * Manages the game engine for conducting races.
 */
public class GameEngine {

    private static GameEngine instance = null;

    /**
     * Private constructor to prevent instantiation from outside the class.
     */
    protected GameEngine() {}

    /**
     * Retrieves the singleton instance of the GameEngine.
     *
     * @return The singleton instance of GameEngine.
     */
    public static GameEngine getInstance() {
        if (instance == null) instance = new GameEngine();
        return instance;
    }

    /**
     * Starts the race for the given competition, iterating through turns until all competitors have finished.
     * Prints the number of turns taken and displays the race results.
     *
     * @param c The competition to start the race for.
     */
    public void startRace(Competition c) {
        int turns = 0;
        while (c.hasCompetitors()) {
            c.playTurn();
            turns++;
        }
        System.out.println("Race finished in " + turns + " steps.");
        System.out.println("Race results:");
        int place = 0;
        for (Competitor c1 : c.getFinishedCompetitors()) {
            place++;
            System.out.println(place + ". " + c1);
        }
    }
}

