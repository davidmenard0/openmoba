
# Game Design Document
The game design document describes the genral design principles of the game.

## Our Approach to Game Design

At its core, OpenMOBA has no pre-defined heroes. Instead, skills are crafted using a skill-crafting system to specialize your build in-game according to your role and strategy. The base skills use skillshots instead of point-and-click mechanics. 


## Accessibility
Accessiblity is a key pillar of game design. We want OpenMOBA to be a true implementation of "easy to learn, hard to master". Unlike classical MOBAs that rely on a huge hero and item pool for gameplay depth, OpenMOBA focuses on skill crafting. 
Every player starts each game as the same base "hero" - an unspecialized generic hero. As a player gains resources, they can pick craft new skills (both permanent and consumable) that specializes them into a certain role, support a specific strategy or counters the enemy choices.
This approach means new players have fewers things to learn before being profficient at the game, while keeping the same depth associated to hundreds of heroes and skills. 


### Depth from customization
Traditional MOBAs add depth through complexity - Hundreds of heroes and skills are a huge barrier to entry. Players are required to learn unique scenarios and interactions in order to play at the highest level, often without any logical reason for them existing. 

Depth in OpenMOBA comes from customization and combination of simple mechanics.

For example, in a traditional MOBA game, your have ~100 heroes with 4 skills each. The number of skills bring huge depth to MOBAs, but add an insane amount of complexity and learning curve before becoming competitive. 

OpenMOBA allows you to create your own skill. Each new option multiplies the permutation of possible skills. For example, 4 skill slots with 4 upgrades with 10 options per upgrade means over 50,000 permutations, with only 10 skill upgrades to learn. 


### Fresh mechanics
Traditional MOBA mechanics are relics of limitations of the first game engine used to create MOBAs. Last hitting, denies, laning, creeps, camp stacking, teleports, click-to-target, gold attribution, etc, are random elements that have been stitched together over the years instead of designed as a whole. 

OpenMOBA rethinks the core mechanics of a MOBA. Modern engines allows us to base our design decisions to optimize them for fun, balance and depth without adding unnecessary or unfriendly complexity.

Skill shots are a key component of OpenMOBA and are the main mechanical skill a player must learn to be proficient at the game.


### A focus on Strategy
OpenMOBA focuses on strategic gameplay and teamwork. Every decision counts and is significant.


### A competitive approach
OpenMOBA is built with competitio and viewership in mind. The objective of each match is to push "the Objective" to the opponent's side by capturing it - standing in it's influence circle without any enemies. 
As the objective is pushed closer to the enemy team's side, the positional advantage is pushed towads the losing team, incentivising conflict. The winning team needs to be decisive and have an edge to assure victory. This also provides a comback mechanic and an exciting viwing experience, because the losing team always has "one last stand", with a positional advantage. 
The game is defsigned to be about 30 minutes per round, with engagement incentivising mechanics appearing more frequently as we approach the 30 minute mark, to force a resolution to the game. 

### Farming
OpenMOBA doesn't rely on farming to get resources. Instead, teams must capture and control areas of the map to generate resources. 

We belive that every action should be the result of a decision. In classic MOBAs, the decision of farming vs fighting is a great pivotal decision, and we want to keep that as a core decision to make. 
But the mechanics of farming (fighting and last hitting creeps) are 90% automatic - your rarely decide to not farm a creep. So we take away the mechanics of farming creeps and replace them with the decision of controling a specific point or area of the map. Just like controling or fighting for a creep camp in classic MOBAs.

Control points give resources to the entire team. But we understand that having different roles and farming patterns are important in a MOBA game. You want your Position 1 player (Carry) to have to make a conscious decision to farm or fight, optimize their farming patterns, and funnel your team's resources into that player.

To acheive these goals, every control point will periodically spawn resources that can be gathered by a player. Only the player who gathers that resource will get the resource, creating a difference in farm between players of the same team, leading to farm priority decisions. 

Additionally, some control points require a player to "buy" them to take control - invest a certain number of personal resources to acquire the point. This allows lower farm priority players (Position 4/5) to control points for their teams and funnel more resources into their Position 1/2 players.

