namespace Application.Constants
{
    /// <summary>
    /// Provides predefined prompts for organizational analysis and job market trend tasks.
    /// </summary>
    /// <remarks>This class contains constant string prompts used for analyzing organizational divisions and
    /// hierarchies. The prompts are designed to guide users in identifying internal divisions responsible for specific
    /// jobs and in researching organizational hierarchies using public sources.</remarks>
    public static class Prompts
    {
        /// <summary>
        /// Represents a constant string that describes the role of an expert in organizational analysis and job market
        /// trends.
        /// </summary>
        public const string DivisionSystem = "You are an expert in organizational analysis and job market trends.";

        /// <summary>
        /// Provides a template message for identifying the most likely internal division or business unit responsible
        /// for a job within a company, based on the job description and publicly available information.
        /// </summary>
        /// <remarks>The message guides the user to analyze the job description and company context to
        /// determine the appropriate division. If no clear division can be identified, the Division value should be
        /// left as an empty string. The result should be returned in a specified JSON format, including the identified
        /// division, reasoning, and a confidence score between 0 and 100.</remarks>
        public const string DivisionMessage = "Given the following:\r\n\r\nCompany Name: {CompanyName}\r\n\r\nJob JobDescription:\r\n{Description}\r\n\r\nYour task is to:\r\n1. Identify the most likely internal division or business unit at the company responsible for this job.\r\n2. Use context from the job description and any publicly available knowledge about the company.\r\n3. If no clear division can be identified, leave the Division value as an empty string.\r\n\r\nReturn the result in the following key-value format:\r\n\r\n{\r\n  \"Division\": \"{{IdentifiedDivision}}\",\r\n  \"Reasoning\": \"{{ShortJustification}}\",\r\n  \"Confidence\": {{ConfidenceScore}}  // value between 0 and 100\r\n}\r\n\r\nDo not include any explanation or extra text outside of the JSON object.";

        /// <summary>
        /// Represents a constant string used to define the role of an expert in organizational hierarchy analysis.
        /// </summary>
        public const string HierarchySystem = "You are an expert in organizational research and data extraction.";

        /// <summary>
        /// Provides a template message for searching public-facing sources to gather information about the
        /// organizational structure of a specified division at a company.
        /// </summary>
        /// <remarks>The message instructs users to search sources such as news articles, press releases,
        /// company websites, or LinkedIn. The expected result format is a JSON object containing an "Org Hierarchy"
        /// array with names and job titles. It emphasizes excluding personal details like emails or phone numbers and
        /// maintaining the hierarchy order from highest to lowest rank.</remarks>
        //public const string HierarchyMessage = "Search public-facing sources such as news articles, press releases, company websites, or LinkedIn to find information about the organizational structure of the {Division} division at {CompanyName}.\r\n\r\nReturn the result in the following format:\r\n\r\n{\r\n  \"Org Hierarchy\": [\r\n    { \"Name\": \"Jane Doe\", \"Title\": \"Senior Vice President, Marketing\" },\r\n    { \"Name\": \"John Smith\", \"Title\": \"Director, Brand Strategy\" },\r\n    { \"Name\": \"Alice Johnson\", \"Title\": \"Marketing Manager\" }\r\n  ]\r\n}\r\n\r\nOnly include names and job titles — do not include emails, phone numbers, or personal details. If there are multiple layers or reporting structures available, maintain the order from highest to lowest rank.\r\n";
        public const string HierarchyMessage = "Here are public LinkedIn profile summaries related to the AI & Analytics Division at Cognizant:{Results}.\r\nExtract an organizational hierarchy in this format:\r\n\r\n{\r\n  \"Org Hierarchy\": [\r\n    { \"Name\": \"John Smith\", \"Title\": \"SVP of AI & Analytics\" },\r\n    { \"Name\": \"Jane Doe\", \"Title\": \"Director of Data Strategy\" }\r\n  ]\r\n}";
    }
}
