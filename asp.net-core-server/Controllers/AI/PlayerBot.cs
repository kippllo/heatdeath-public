using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Keybored.BackendServer.GameLevel;
using Keybored.BackendServer.GameEngine;
using Keybored.BackendServer.Network;
using Keybored.BackendServer.Settings;

namespace Keybored.BackendServer.AI {

    public class PlayerBot: connection { //Inherit from the player connection class, this will allow all of the bots to be included in the main "getClientSyncObjects" loop without any extra code!
        
        //Note: The "playerObject" property is inherited from the "connection" class...
        private List<playerObj> playersInView;
        private List<bulletObj> bulletsInView;
        
        private Vector3Thin aimTarget;
        private Vector3Thin moveTarget;

        float dangerRadius; // Radius that is safe from the circle center...
        Vector3Thin dangerCenter; //Center of safety circle...
        Vector3Thin closestPlayer; // Will be "null" if no player is in view...
        List<cube> wallsInView; //START HERE BY FILLING THIS IN THE START OF UPDATE.
        List<Vector3Thin> pathfindingSteps;
        
        private float speed = 5.0f;
        private float shootTimer = 0f;
        private float shootTimerMax = 0.175f; //WAS: 0.15f;

        private float partResetTimer = 0f;
        private float partResetTimerMax = 0.025f;
        
        // These two settings are needed for giving bullets a proper net object ID...
        private int syncNextObjID = 0;
        
        private syncLevelData _levelData;
        public syncLevelData levelData { //Public set only for now because the game needs to be able to set this, maybe change later...
            get { return _levelData; } //Maybe remove later...
            set {
                _levelData = value;
            }
        }

        private MapData mapData; //Just a quick hac for now. This is set in "Game.addConnectionBot()"...

        enum MovementStrat { Destroyer, Camper, Bounce, Orbit }; //enum MovementStrat { Destroyer, Camper, Runner };
        enum AttackStrat { Beam, Spread, Spin }; //Maybe add: (shootStar/shootCross "+" shape)
        MovementStrat movementStrat;
        AttackStrat attackStrat;

        // Maybe user a dictionary for these below later if I need a lot of variables and naming them would be beneficial...
        private float[] moveBehaviorVars; // These two are extra arrays that hold frame-persistent variables for the AI Behavior functions.
        private float[] aimBehaviorVars; //  Note: They should be left "null" until the Behavior function initializes them how it wants them!
        

        public PlayerBot(): this(null, -1, null, null){
        }
        public PlayerBot(Game gameIN, int indexIN): this(gameIN, indexIN, null, null){ }

        public PlayerBot(Game gameIN, int indexIN, MapData mapDataIN): this(gameIN, indexIN, null, mapDataIN){ } //This is the one being used rn...

        public PlayerBot(Game gameIN, int indexIN, syncLevelData levelDataIN, MapData mapDataIN): base(gameIN, indexIN, ""){
            playerObject = new playerObj();
            playerObject.hp = 100; //Make sure this matches max player HP...


            //Spawn bot at a random spawn point...
            mapData = mapDataIN;
            Random rand = new Random();
			int randInd = rand.Next(0, mapData.spawnPoints.Length);
            Vector3Thin spawnPoint = mapData.spawnPoints[randInd];

            playerObject.pos.x = spawnPoint.x + GameMath.randomRange(-mapData.width/4, mapData.width/4);
            playerObject.pos.y = spawnPoint.y + GameMath.randomRange(-mapData.height/4, mapData.height/4);
            playerObject.pos.z = spawnPoint.z;

            playerObject.username = "Bot_" + index;
            playerObject.objID = getNextObjID();

            maxObjDist = ServerSettings.objRenderDist;
            aimTarget = new Vector3Thin();
            moveTarget = playerObject.pos.Clone();
            //Dno't set "pntOfWallCache" in the constructor. It is set in the movement functions...
            playersInView = new List<playerObj>();
            bulletsInView = new List<bulletObj>();
            wallsInView = new List<cube>();
            pathfindingSteps = new List<Vector3Thin>();
            levelData = levelDataIN;

            player = JsonConvert.SerializeObject(playerObject, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            //Trying to define these because of an error...
            dangerCenter = playerObject.pos;

            // Setup Bot behavior...
			randInd = rand.Next(0, Enum.GetNames(typeof(MovementStrat)).Length);
            movementStrat = (MovementStrat)randInd;

            randInd = rand.Next(0, Enum.GetNames(typeof(AttackStrat)).Length);
            attackStrat = (AttackStrat)randInd;
            
            ////movementStrat = MovementStrat.Orbit; //Debug remove later!
            ////attackStrat = AttackStrat.Spread; //Debug remove later!
        }

        
        public void updateBot(float dt){
            if(_levelData == null || playerObject.hp <= 0) return; //Don't try to update if the level data has not been set yet...

            // Reset particles...
            partResetTimer += dt;
            if(partResetTimer >= partResetTimerMax){
                partResetTimer -= partResetTimerMax;
                playerObject.partsysFireEmit = false;
                playerObject.partsysDamageEmit = false;
                playerObject.flgSoundDeath = false;
            }

            //Fill the playersInView list...
            playersInView.Clear(); //Clear the last frame's vision...
            bulletsInView.Clear();
            int connectionsLen = game.connections.Count;
            for(int c=1; c<connectionsLen; c++){ // "c=1" to avoid dummy...
                if(c != index){ // Don't add this bot to its own "playersInView" list...
                    string getPlayerCache = game.connections[c].getPlayer(playerObject.pos);
                    if(getPlayerCache != "") { // If the player would return data for another real player, add it to this bot's "InView" list.
                        playersInView.Add(game.connections[c].playerObject);
                    }

                    //While we are at it, fill which bullets are in the AI's view as well...
                    bulletsInView.AddRange(game.connections[c].getBulletsObjList(playerObject.pos));
                }
            }
            
            
            //Also grab bullets the other bots have shot as well...
            bulletsInView.AddRange(game.returnBulletsInView(this));

            //Update the closest player var
            closestPlayer = closestPlayerPos();
            
            // Move bot
            moveBot(dt);

            // Shoot bullets, the timer is inside of the function.
            shootBullet(dt);

            collisionCheck(dt);

            player = JsonConvert.SerializeObject(playerObject, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore }); // Note: This takes the place of "connection.renderBufferFrame()". This line updates "connection.player" so that "connection.getPlayer()" will return the correct data for this bot.
            
            lastSyncTime = DateTimeOffset.Now.ToUnixTimeMilliseconds(); //Update the last sync time. I'm using this for now to because as long as a player is watching the "bot's only" match this number will keep showing that the bot is active. But if there are no player's watching, this number will show that the bot is inactive!
        }

        
        private void moveBot(float dt){
            switch (movementStrat){
                case MovementStrat.Destroyer:
                    moveDestroyer(dt);
                    break;
                case MovementStrat.Camper:
                    moveCamper(dt);
                    break;
                case MovementStrat.Bounce:
                    moveBounce(dt);
                    break;
                case MovementStrat.Orbit:
                    moveOrbit(dt);
                    break;
            }
        }
        
        private void shootBullet(float dt){
            // Shoot timer is inside of the shoot-type functions.
            switch (attackStrat){
                case AttackStrat.Beam:
                    shootBeam(dt);
                    break;
                case AttackStrat.Spread:
                    shootSpread(dt);
                    break;
                case AttackStrat.Spin:
                    shootSpin(dt);
                    break;
            }
        }

        private void collisionCheck(float dt){
            //Just do a fast sphere check...
            float hitboxWidth = 1.25f;

            //Check for hit damage...
            int len = bulletsInView.Count;
            for(int b=0; b<len; b++){
                bulletObj pew = bulletsInView[b];
                if(Vector3Thin.distance(playerObject.pos, pew.pos) <= hitboxWidth){
                    playerObject.hp -= 25f*dt; //This will need to be adjusted!
                    playerObject.partsysDamageEmit = true;
                }
            }

            // Check if the bot is outside of the safety circle, if it is take damage...
            float lerpAmount = _levelData.circleUpdateTimer/_levelData.circleUpdateTimerMax;
            dangerRadius = GameMath.lerp(_levelData.safeDist, _levelData.NextSafeDist, lerpAmount); //Note: These are in reverse order in the lerp as they are on the client side...
            dangerCenter = Vector3Thin.lerp(_levelData.safeCenter, _levelData.NextSafeCenter, lerpAmount);
            if (Vector3Thin.distance(dangerCenter, playerObject.pos) > dangerRadius){
                playerObject.hp -= 5*dt;
            }

            if(playerObject.hp <= 0){
                // If the bot has been killed send the network flag to play the death sound.
                playerObject.flgSoundDeath = true;
            }
        }



        //--------------------------------------------------------------
        //      MOVEMENT FUNCTIONS--------------------------------------------------------------
        //--------------------------------------------------------------
        // NOTE: Movement behaviors will all use the same pathfinding and collision function, but will be different by how the bot finds its target.

        private void moveDestroyer(float dt){
            bool goalMoved = false;

            if(Vector3Thin.distance(playerObject.pos, moveTarget) < 1){
                //Reset move target...
                moveTarget = closestPlayer.Clone();
                goalMoved = true;
            }

            //If a new target is closer than the current one, switch to following it.
            float curTargetDist = Vector3Thin.distance(playerObject.pos, moveTarget);
            float closestPlayerDist = Vector3Thin.distance(playerObject.pos, closestPlayer);
            if(closestPlayerDist < curTargetDist) {
                moveTarget = closestPlayer.Clone();
                goalMoved = true;
            }

            playerObject.pos = moveCollisionCheck(dt, moveTarget, goalMoved);
        }


        // NOTE: "nextMovePoint" might just need to be "moveTarget" so the bot can tell if any walls are in the way of its current movement path...
        private Vector3Thin moveCollisionCheck(float dt, Vector3Thin nextMovePoint, bool goalMoved){
            bool moveHitsWall = false;
            
            float lerpDist = Vector3Thin.distance(playerObject.pos, nextMovePoint);
            lerpDist = GameMath.clamp(lerpDist, 1.5f, 150);
            float lerp = speed*dt/lerpDist; //When the distant is small, the lerp will be big. When the distance is big the lerp will be small. But there are caps!
            //Below's "nextStep" is the next movement step that does not use pathfinding!
            Vector3Thin nextStep = Vector3Thin.lerp(playerObject.pos, nextMovePoint, lerp);
            moveHitsWall = mapData.posHitsWall(nextStep);

            if(goalMoved) pathfindingSteps.Clear(); // If the bot's goal has moved since the last frame, reset pathfinding!

            if(moveHitsWall && pathfindingSteps.Count < 1){
                pathfindingSteps = AStar.FindPath(playerObject.pos, moveTarget, mapData);
            }
            if(moveHitsWall || pathfindingSteps.Count > 0) {
                if(pathfindingSteps.Count > 0){
                    lerpDist = Vector3Thin.distance(playerObject.pos, pathfindingSteps[0]);
                    lerpDist = GameMath.clamp(lerpDist, 1.5f, 150);
                    lerp = (speed*1.5f)*dt/lerpDist;
                    nextStep = Vector3Thin.lerp(playerObject.pos, pathfindingSteps[0], lerp);

                    // Keep this down here so we don't accidentally remove the last step before the above lerp references it.
                    if(Vector3Thin.distance(playerObject.pos, pathfindingSteps[0]) < 0.25f){ //Remove the first pathfinding step if we are close to being on top of it.
                        pathfindingSteps.RemoveAt(0); // Maybe change to a queue later...
                    }
                }
            }

            return nextStep;
        }

        // Seek goal like a Destroyer but
        // orbit around the closest player or goal.
        private void moveOrbit(float dt){
            if(moveBehaviorVars == null){
                moveBehaviorVars = new float[3];
                moveBehaviorVars[0] = 0; //Holds orbit's corrent rotation degrees.
                moveBehaviorVars[1] = 0; //Holds orbit movement timer.
            }
            
            bool goalMoved = false;
            float NextSafeDist = _levelData.NextSafeDist;
            Vector3Thin NextSafeCenter = _levelData.NextSafeCenter;

            float orbitRadius = 10f;
            float orbitRotDegrees = moveBehaviorVars[0];
            float orbitRotStep = 10f; //10 degrees.
            float orbitTimer = moveBehaviorVars[1];
            float orbitTimerMax = 0.25f; // In seconds.

            bool targetIsTooFar = Vector3Thin.distance(playerObject.pos, moveTarget) > orbitRadius*2f; //Maybe try 1.5f???
            bool targetNeedsRefocus = Vector3Thin.distance(playerObject.pos, moveTarget) < orbitRotStep+1;

            if(targetNeedsRefocus){
                moveTarget = closestPlayer.Clone();
                goalMoved = true;
            }

            // Rotation controls.
            orbitTimer += dt;
            if(orbitTimer > orbitTimerMax){
                orbitTimer -= orbitTimerMax;
                orbitRotDegrees += orbitRotStep;
            }
            Vector3Thin baseRotPoint = new Vector3Thin(moveTarget.x + orbitRadius, moveTarget.y);
            Vector3Thin rotTarget = GameMath.rotatePointByLocalDegrees(moveTarget, baseRotPoint, orbitRotDegrees);
            
            // Only orbit if we are close to the target...
            if(targetIsTooFar){
                // We are too far away from the target, just move in a straight line towards the target.
                playerObject.pos = moveCollisionCheck(dt, moveTarget, goalMoved);
            } else {
                // We are close to target, move in an orbit around the target.
                // Don't lerp towards the main "moveTarget", but instead lerp towards the "rotationTarget"
                // which is based on that moveTarget...
                playerObject.pos = moveCollisionCheck(dt, rotTarget, goalMoved);
            }
            
            
            // Save all frame-persistent variables back into the array!
            moveBehaviorVars[0] = orbitRotDegrees;
            moveBehaviorVars[1] = orbitTimer;
        }


        private void moveCamper(float dt){
            float tooCloseDist = 20f;
            bool goalMoved = false;
            float NextSafeDist = _levelData.NextSafeDist;
            Vector3Thin NextSafeCenter = _levelData.NextSafeCenter;

            bool targetOutOfCircle = Vector3Thin.distance(moveTarget, NextSafeCenter) > NextSafeDist; //If the target is outside of the next saftey zone.
            bool shouldRun = (playersInView.Count > 0) ? Vector3Thin.distance(moveTarget, closestPlayer) < tooCloseDist : false; //If there is a player too close, run from him!

            if(targetOutOfCircle || shouldRun){
                //Reset to a new default target...
                float randSignX = GameMath.Sign(GameMath.randomRange(-1, 1));
                float randSignY = GameMath.Sign(GameMath.randomRange(-1, 1));

                float randX = GameMath.randomRange(0f, NextSafeDist); //Between 0-max.
                float radiusPercentLeft = 1f-(randX/NextSafeDist); //The percent of the radius left to use after the X length is taken out.
                float randY = NextSafeDist * radiusPercentLeft; //Set Y to use what's left of the radius.

                //Apply the direction signs...
                randX *= randSignX;
                randY *= randSignY;
                
                moveTarget = new Vector3Thin(NextSafeCenter.x + randX, NextSafeCenter.y + randY);
                goalMoved = true;
            }

            playerObject.pos = moveCollisionCheck(dt, moveTarget, goalMoved);
        }


        // Bot will bounce round the edges of the saftey circle, never chasing any players!
        private void moveBounce(float dt){
            float arrivedDist = 5f;
            bool goalMoved = false;
            float NextSafeDist = _levelData.NextSafeDist;
            Vector3Thin NextSafeCenter = _levelData.NextSafeCenter;

            bool targetOutOfCircle = Vector3Thin.distance(moveTarget, NextSafeCenter) > NextSafeDist; //If the target is outside of the next saftey zone.
            bool reachedTarget = Vector3Thin.distance(playerObject.pos, moveTarget) < arrivedDist;

            if(reachedTarget || targetOutOfCircle){
                //Reset to a new default target...
                float randSignX = GameMath.Sign(GameMath.randomRange(-1, 1));
                float randSignY = GameMath.Sign(GameMath.randomRange(-1, 1));

                float randX = GameMath.randomRange(0f, NextSafeDist); //Between 0-max.
                float radiusPercentLeft = 1f-(randX/NextSafeDist); //The percent of the radius left to use after the X length is taken out.
                float randY = NextSafeDist * radiusPercentLeft; //Set Y to use what's left of the radius.

                //Apply the direction signs...
                randX *= randSignX;
                randY *= randSignY;
                
                moveTarget = new Vector3Thin(NextSafeCenter.x + randX, NextSafeCenter.y + randY);
                goalMoved = true;
            }

            playerObject.pos = moveCollisionCheck(dt, moveTarget, goalMoved);
        }
        //--------------------------------------------------------------
        //      END MOVEMENT FUNCTIONS--------------------------------------------------------------
        //--------------------------------------------------------------


        //--------------------------------------------------------------
        //      SHOOT FUNCTIONS--------------------------------------------------------------
        //--------------------------------------------------------------
        private void spawnBullet(){
            // Spawn a new bullet and give it a copy of the bot's position and rotation.
            bulletObj pew = new bulletObj();
            pew.pos = playerObject.pos.Clone();
            pew.rot = playerObject.rot.Clone();
            pew.objID = getNextObjID(); //Set the bullet's net ObjectID...
            game.bullets.Add(pew);
            playerObject.partsysFireEmit = true;
        }


        private void shootBeam(float dt){
            aimTarget = closestPlayer.Clone();
            // Always Update bot rotation, even outside of the timer...
            Vector3Thin moveRot = new Vector3Thin();
            moveRot.z = GameMath.pointZAxisTowards(playerObject.pos, aimTarget); //Only rotate on the Z axis...
            playerObject.rot = moveRot;
            
            shootTimer += dt;
            if(shootTimer >= shootTimerMax){
                shootTimer -= shootTimerMax;

                // If the bot can see a player, shoot at him!
                if(playersInView.Count > 0){
                    spawnBullet();
                }
            }
        }


        private void shootSpread(float dt){
            if(aimBehaviorVars == null){
                // Init the frame-persistent extra variable array.
                // We only need one extra variable for this specific behavior.
                aimBehaviorVars = new float[1];
                aimBehaviorVars[0] = 1; //This var will be the spin direction tracker to keep the bot spinning the correct way! It will always either be "-1" or "1".
            }

            float rotSign = aimBehaviorVars[0]; //Grab the rotation dirction sign from the frame-persistent variables.
            float maxDegreeRot = 40;
            float degreeRotStep = 10;

            bool playerIsClose = playersInView.Count > 0;
            aimTarget = closestPlayer.Clone();
            
            shootTimer += dt;
            if(shootTimer >= shootTimerMax){
                shootTimer -= shootTimerMax;
                if(playerIsClose){
                    spawnBullet();
                    
                    Vector3Thin moveRot = new Vector3Thin();
                    moveRot.z = playerObject.rot.z + (degreeRotStep*rotSign);
                    float targetZ = GameMath.pointZAxisTowards(playerObject.pos, aimTarget); //Where this bot should rotate to actually be pointing at the target.

                    if(moveRot.z < targetZ+(maxDegreeRot*rotSign)){
                        rotSign = +1; //Flip the rotation sign.
                    } else if(moveRot.z > targetZ+(maxDegreeRot*rotSign)){
                        rotSign = -1;
                    }

                    if( Math.Abs(targetZ-moveRot.z) > maxDegreeRot ){
                        // If the difference in the bot's current rotation and the needed Point-At-Target's rotation is greater
                        // than "maxDegreeRot", then reset the bot's rotation to be pointed at the target!
                        moveRot.z = targetZ;
                    }
                    
                    playerObject.rot = moveRot; //Set the bots rotation after all calculations are done.

                } else {
                    //Aim at the move target and don't shoot any bullets.
                    Vector3Thin moveRot = new Vector3Thin();
                    moveRot.z = GameMath.pointZAxisTowards(playerObject.pos, aimTarget); //Only rotate on the Z axis...
                    playerObject.rot = moveRot;
                }
            }

            //Save all frame-persistent variables back into the array!
            aimBehaviorVars[0] = rotSign;
        }


        private void shootSpin(float dt){
            float degreeRotStep = 45;
            bool playerIsClose = playersInView.Count > 0;
            aimTarget = closestPlayer.Clone();
            
            shootTimer += dt;
            if(shootTimer >= shootTimerMax){
                shootTimer -= shootTimerMax;
                if(playerIsClose){
                    spawnBullet();
                    Vector3Thin moveRot = new Vector3Thin();
                    moveRot.z = playerObject.rot.z + degreeRotStep;
                    playerObject.rot = moveRot;
                } else {
                    Vector3Thin moveRot = new Vector3Thin();
                    moveRot.z = GameMath.pointZAxisTowards(playerObject.pos, aimTarget);
                    playerObject.rot = moveRot;
                }
            }
        }


        private Vector3Thin closestPlayerPos(){
            // If there are not any players in view, return the center of the zone circle.
            // Else return the closest player in view.
            Vector3Thin rtnPos;
            if(playersInView.Count == 0){
                rtnPos = dangerCenter.Clone();
            } else {
                //First set the "targetPos" to a default value of the first index of the players in view:
                rtnPos = playersInView[0].pos.Clone();

                //Now find the closest player.
                int playersInViewLen = playersInView.Count;
                for(int p=0; p<playersInViewLen; p++){
                    playerObj playerInd = playersInView[p];
                    float distToCurrTarget = Vector3Thin.distance(playerObject.pos, rtnPos);
                    float distToNewTarget = Vector3Thin.distance(playerObject.pos, playerInd.pos);
                    if( distToNewTarget < distToCurrTarget ){
                        rtnPos = playerInd.pos.Clone();
                    }
                }
            }

            return rtnPos;
        }

        //--------------------------------------------------------------
        //      END SHOOT FUNCTIONS--------------------------------------------------------------
        //--------------------------------------------------------------




        // Override the origin functions that don't apply to the bots.
        // Replace them with empty functions that don't do anything.
        //
        // It might be best to make "connection" an abstract class later, and make a new "ConnectionPlayer" and "ConnectionBot" that inherit from that...
        // That would allow the bots to take up less memory because they won't create a "syncSendBuffer" and the like...
        public override void updateObjects(string playerData, string bulletsData) { return; }
        public override void addToSendBuffer(sendClientData netFrame){ return; }
        public override void renderBufferFrame(){ return; }
        public override string getBullets(Vector3Thin checkPos) { return ""; } //always return blank bullets because the "game" object will fill these for the class.

        public override List<bulletObj> getBulletsObjList(Vector3Thin checkPos) { return new List<bulletObj>(); }
        /* I think I need to override "connection.getBulletsObjList()" to return nothing too because all bot bullets should be returned by the "game.returnBulletsInView()". If both of these are called that means the both will be sent double of each bot-in-view's bullets...
           It should be fine to override this function since real players call "game.strReturnBulletsInView()" to get bot bullets.
        */
        
        /* NOTE: The below is outdated. Now the bot does need to take "connection.isActive" into account when the deciding to end the game!
        public override bool isActive { //Don't take the "lastSyncTime" into account on bot players...
            get { return playerObject.hp > 0; }
        } */


        //
        // Other Functions:
        //
        private int getNextObjID(){
            int nextID = syncNextObjID++;
            string strID = ""+ nextID;
            return int.Parse(index + strID.PadLeft(NetworkObj.ObjIDLimit, '0'));
        }

    }

}