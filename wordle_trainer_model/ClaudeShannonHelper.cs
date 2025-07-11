namespace wordle_trainer_model
{
    public static class ClaudeShannonHelper
    {
        public static double ComputeUncertainty(ICollection<string> solution_space)
        {
            if (solution_space is null || solution_space.Count == 0) return 0.0f;
            double uncertainty = 0.0f;
            uncertainty = Math.Log2(solution_space.Count);
            return uncertainty;
        }

        public static double ComputyGainedInformation(ICollection<string> old_solution_space, ICollection<string> new_solution_space)
        {
            double gained_information = 0.0f;
            gained_information = ComputeUncertainty(old_solution_space) - ComputeUncertainty(new_solution_space);
            return gained_information;
        }

        public static double ComputeLikelihood(ICollection<string> old_solution_space, ICollection<string> new_solution_space)
        {
            return ((double)new_solution_space.Count) / (double)(old_solution_space.Count);
        }
    }
}