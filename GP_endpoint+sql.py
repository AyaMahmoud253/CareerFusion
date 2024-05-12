from flask import Flask, jsonify
import pandas as pd
from sklearn.feature_extraction.text import CountVectorizer
from sklearn.metrics.pairwise import cosine_similarity
import pyodbc
import string
from nltk.corpus import stopwords
from nltk.tokenize import word_tokenize
import spacy

# Download NLTK resources
import nltk
nltk.download('punkt')
nltk.download('stopwords')

# Load SpaCy model
nlp = spacy.load("en_core_web_md")

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
query_job_descriptions = "SELECT * FROM JobDescriptions"

# Load data into DataFrames
users = read_sql_query(query_users, conn)
skills = read_sql_query(query_skills, conn)
jobs = read_sql_query(query_jobs, conn)
job_skills = read_sql_query(query_job_skills, conn)
job_descriptions = read_sql_query(query_job_descriptions, conn)

# Preprocessing
# Combine user skills
users['CombinedSkills'] = users['Id'].map(skills.groupby('UserId')['SkillName'].apply(', '.join))
users['CombinedSkills'] = users['CombinedSkills'].fillna('')

# Define the function to compute similarity
def compute_similarity(user_text, job_text):
    vectorizer = CountVectorizer()
    text_matrix = vectorizer.fit_transform([user_text, job_text])
    cosine_sim = cosine_similarity(text_matrix)
    return cosine_sim[0, 1]

# Text preprocessing function
def preprocess_text(text):
    # Convert text to lowercase
    text = text.lower()
    # Remove punctuation
    text = text.translate(str.maketrans('', '', string.punctuation))
    # Tokenize text
    tokens = word_tokenize(text)
    # Remove stopwords
    stop_words = set(stopwords.words('english'))
    tokens = [word for word in tokens if word not in stop_words]
    # Return preprocessed text as string
    return ' '.join(tokens)

# Flask App
app = Flask(__name__)

# Define endpoint for job recommendations
@app.route('/recommend-jobs/<string:user_id>', methods=['GET'])
def recommend_jobs(user_id):
    # Fetch user's information from the DataFrame
    user_data = users[users['Id'] == user_id].iloc[0]

    # Check if the user has all the required skills for the job
    if 'CombinedSkills' not in user_data:
        return jsonify({'message': 'User has no skills'}), 400

    # Generate user description from user's title and skills
    user_text = preprocess_text(f"{user_data['Title']} {user_data['CombinedSkills']} {user_data['Address']}")

    # Maintain a list of recommended jobs
    recommended_jobs = []

    # Iterate through jobs and compute similarity
    for index, job in jobs.iterrows():
        job_id = job['Id']
        job_title = job['JobTitle']
        job_skill_query = job_skills[job_skills['JobFormEntityId'] == job_id]
        job_skills_list = job_skill_query['SkillName'].tolist()
        job_skills_text = ', '.join(job_skills_list)

        # Generate job description from job title and skills
        job_description_query = job_descriptions[job_descriptions['JobFormEntityId'] == job_id]
        if not job_description_query.empty:
            job_description = job_description_query.iloc[0]['Description']
        else:
            job_description = ''

        job_text = preprocess_text(f"{job_title} {job_skills_text} {job_description}")

        # Compute similarity
        similarity = compute_similarity(user_text, job_text)

        # Check if the user has all the required skills for the job
        user_skills_set = set(user_data['CombinedSkills'].split(', '))
        job_skills_set = set(job_skills_list)
        if user_skills_set.issuperset(job_skills_set):
            similarity = 1.0  # Set similarity to maximum if user has all required skills

        # Add job to recommended list if similarity is above threshold
        if similarity >= 0.7:
            recommended_jobs.append({
                'JobId': job_id,
                'JobTitle': job_title,
                'Similarity': similarity
            })

    # Sort recommended jobs by similarity in descending order
    recommended_jobs = sorted(recommended_jobs, key=lambda x: x['Similarity'], reverse=True)

    # Return top 10 recommended jobs
    top_10_jobs = recommended_jobs[:10]

    return jsonify(top_10_jobs)

if __name__ == '__main__':
    app.run(debug=True)
