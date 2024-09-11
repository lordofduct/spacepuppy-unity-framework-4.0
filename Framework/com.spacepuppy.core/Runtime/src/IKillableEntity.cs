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

    public interface IOnKillHandler
    {
        /// <summary>
        /// Called in reverse order if no components 'Cancelled' the kill during OnPreKill.
        /// </summary>
        void OnKill(KillableEntityToken token);
    }

    /// <summary>
    /// Implement this interface if you want to write a script that handles how the entity is dealt with when 'Kill' is called on it. 
    /// This overrides the default behaviour of destroying the GameObject and child GameObjects. 
    /// 
    /// This means generally an IKIllableEntity does something in place of Destroying it, or handles the destroying itself. 
    /// 
    /// There is an election process for selecting which IKillableEntity script handles the "Kill" of the object.
    /// First all components implementing IKillableEntity will have OnPreKill called on them allowing the component to either propose itself as a candidate or to cancel the kill altogether. 
    /// Next OnKill (inherited from IOnKillHandler) is called, the included token will point to the candiate elected or null of Destroy is going to be called.
    /// Finally OnElectedKillCandidate is called if that component was the elected candidate.
    /// </summary>
    public interface IKillableEntity : IOnKillHandler, IComponent
    {

        /// <summary>
        /// Returns true if the entity is in a state considered "dead".
        /// </summary>
        bool IsDead { get; }

        /// <summary>
        /// Called on all IKillableEntity components in reverse order to allow each component to attempt to cancel/override the kill behavior. 
        /// This method is not guaranteed to be called, sometimes an object is going to be killed regardless and only 'OnKill' is called. 
        /// So don't rely on this method being called and instead only handle canceling of kill OR electing to be kill candidate.
        /// </summary>
        /// <param name="token"></param>
        void OnPreKill(ref KillableEntityToken token, GameObject target);

        /// <summary>
        /// Called if this component called ProposeKillCandidate during OnPreKill and won the priority contest.
        /// </summary>
        /// <param name="token"></param>
        void OnElectedKillCandidate();

    }

}

