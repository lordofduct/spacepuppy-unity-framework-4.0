using com.spacepuppy.Collections;
using UnityEngine;

namespace com.spacepuppy
{

    public struct KillableEntityToken
    {

        public bool Cancelled { get; private set; }

        public IKillableEntity Candidate { get; private set; }

        public float CandidatePriority { get; private set; }

        /// <summary>
        /// Proposes a component to handle the 'killing' of the entity. If the candidate is the winner (has highest priority), 
        /// it will have 'OnElectedKillCandidate' called on it so it can handle the act of destroying the entity.
        /// </summary>
        public void ProposeKillCandidate(IKillableEntity candidate, float priority)
        {
            if (this.Candidate == null || priority > this.CandidatePriority)
            {
                this.Candidate = candidate;
                this.CandidatePriority = priority;
            }
        }

        public void Cancel()
        {
            this.Cancelled = true;
        }

    }

    /// <summary>
    /// Implement this interface if you want to write a script that handles how the entity is dealt with when 'Kill' is called on it. 
    /// This overrides the default behaviour of destroying the GameObject and child GameObjects. 
    /// 
    /// This means generally a IKIllableEntity does something in place of Destroying it, or handles the destroying itself. It should return 
    /// true if it successfully did this. If all IKillableEntity scripts on an entity return 'false', then Object.Destroy will destroy the object. 
    /// If any 1 IKillableEntity script on an entity returns 'true', then Destroy will NOT be called.
    /// </summary>
    public interface IKillableEntity : IComponent
    {

        /// <summary>
        /// Returns true if the entity is in a state considered "dead".
        /// </summary>
        bool IsDead { get; }

        /// <summary>
        /// Called on all IKillableEntity components in reverse order to allow each component to attempt to cancel/override the kill behavior.
        /// </summary>
        /// <param name="token"></param>
        void OnPreKill(ref KillableEntityToken token, GameObject target);

        /// <summary>
        /// Called on all IKillableEntity components in reverse order if no components 'Cancelled' the kill during OnPreKill.
        /// </summary>
        void OnKill(KillableEntityToken token);

        /// <summary>
        /// Called if this component called ProposeKillCandidate during OnPreKill and won the priority contest.
        /// </summary>
        /// <param name="token"></param>
        void OnElectedKillCandidate();

    }

}

