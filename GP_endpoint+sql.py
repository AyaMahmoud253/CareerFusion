from flask import Flask, jsonify
import pandas as pd
from sklearn.feature_extraction.text import CountVectorizer
from sklearn.metrics.pairwise import cosine_similarity
import pyodbc

# Establish a connection to SQL Server
conn_str = (
    r"Driver={ODBC Driver 17 for SQL Server};"
    r"Server=DESKTOP-5BL7H39;"
    r"Database=UserApp;"
    r"Trusted_Connection=yes;"
    r"TrustServerCertificate=Yes;"
)
conn = pyodbc.connect(conn_str)

# Define a function to handle reading data into DataFrames
def read_sql_query(query, conn):
    try:
        return pd.read_sql(query, conn, coerce_float=False)
    except Exception as e:
        print(f"Error reading query: {query}")
        print(f"Error message: {str(e)}")
        return pd.DataFrame()  # Return an empty DataFrame on error

# Query the data for each table
query_users = """
    SELECT Id, Title, Address
    FROM AspNetUsers
"""

query_skills = "SELECT * FROM Skills"
query_jobs = "SELECT * FROM JobForms"
query_job_skills = "SELECT * FROM JobSkills"

# Load data into DataFrames
users = read_sql_query(query_users, conn)
skills = read_sql_query(query_skills, conn)
jobs = read_sql_query(query_jobs, conn)
job_skills = read_sql_query(query_job_skills, conn)

# Preprocessing
# Combine user skills
users['CombinedSkills'] = users['Id'].map(skills.groupby('UserId')['SkillName'].apply(','.join))
users['CombinedSkills'] = users['CombinedSkills'].fillna('')

# Define the function to compute similarity
def compute_similarity(user_skill, job_skill):
    vectorizer = CountVectorizer()
    skill_matrix = vectorizer.fit_transform([user_skill, job_skill])
    cosine_sim = cosine_similarity(skill_matrix)
    return cosine_sim[0, 1]

# Flask App
app = Flask(__name__)

# Define endpoint for job recommendations
@app.route('/recommend-jobs/<string:user_id>', methods=['GET'])
def recommend_jobs(user_id):
    # Fetch user's skill from the database
    user_skills_query = f"""
        SELECT SkillName
        FROM Skills
        WHERE UserId = '{user_id}'
    """
    user_skills_df = read_sql_query(user_skills_query, conn)
    user_skill = set(user_skills_df['SkillName']) if not user_skills_df.empty else set()

    # Maintain a set of recommended job IDs
    recommended_job_ids = set()

    # Output jobs where the user has all required skills first
    recommended_jobs = []
    for index, job in jobs.iterrows():
        job_id = job['Id']
        job_skills_query = f"""
            SELECT SkillName
            FROM JobSkills
            WHERE JobFormEntityId = {job_id}
        """
        job_skills_df = read_sql_query(job_skills_query, conn)
        job_skill = set(job_skills_df['SkillName']) if not job_skills_df.empty else set()
        if user_skill.issuperset(job_skill):
            job_data = job.to_dict()
            job_data['Similarity'] = 1.0  # Maximum similarity when user has all required skills
            recommended_jobs.append(job_data)
            recommended_job_ids.add(job_id)

    # Compute similarity between user skill and each job's required skill
    for index, job in jobs.iterrows():
        job_id = job['Id']
        if job_id in recommended_job_ids:
            continue  # Skip jobs already recommended
        job_skills_query = f"""
            SELECT SkillName
            FROM JobSkills
            WHERE JobFormEntityId = {job_id}
        """
        job_skills_df = read_sql_query(job_skills_query, conn)
        job_skill = set(job_skills_df['SkillName']) if not job_skills_df.empty else set()
        similarity = compute_similarity(','.join(user_skill), ','.join(job_skill))
        if similarity > 0:  # Exclude jobs with similarity score of 0
            job_data = job.to_dict()
            job_data['Similarity'] = similarity
            recommended_jobs.append(job_data)

    return jsonify(recommended_jobs)

if __name__ == '__main__':
    app.run(debug=True)
