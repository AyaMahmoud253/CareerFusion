from flask import Flask, jsonify
from flask_cors import CORS
import requests
import pandas as pd
from sklearn.feature_extraction.text import CountVectorizer
from sklearn.metrics.pairwise import cosine_similarity
import string
from nltk.corpus import stopwords
from nltk.tokenize import word_tokenize
import spacy
import nltk

# Download NLTK resources
nltk.download('punkt')
nltk.download('stopwords')

# Load SpaCy model
nlp = spacy.load("en_core_web_md")

# Base URL for .NET API
BASE_URL = "http://localhost:5266/api/ForRecommend"

# Function to fetch data from .NET API
def fetch_data(endpoint):
    try:
        response = requests.get(f"{BASE_URL}/{endpoint}")
        response.raise_for_status()
        return response.json()
    except requests.exceptions.RequestException as e:
        print(f"Error fetching {endpoint}: {e}")
        return []

# Fetch data from .NET API endpoints
users = fetch_data("users")
jobs = fetch_data("jobs")

# Debugging: Print the fetched user data
print("Users data:", users)

# Convert user data to DataFrame
users_df = pd.DataFrame(users)

# Rename columns to match JSON keys
users_df.rename(columns={
    'id': 'Id',
    'title': 'Title',
    'address': 'Address',
    'combinedSkills': 'CombinedSkills'
}, inplace=True)

# Ensure CombinedSkills column exists
if 'CombinedSkills' not in users_df.columns:
    users_df['CombinedSkills'] = ''

# Preprocessing
users_df['CombinedSkills'] = users_df['CombinedSkills'].fillna('')

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
CORS(app)  # Enable CORS for the Flask app

# Define endpoint for job recommendations
@app.route('/recommend-jobs/<string:user_id>', methods=['GET'])
def recommend_jobs(user_id):
    # Fetch user's information from the DataFrame
    user_data = users_df[users_df['Id'] == user_id].iloc[0]

    # Check if the user has all the required skills for the job
    if 'CombinedSkills' not in user_data:
        return jsonify({'message': 'User has no skills'}), 400

    # Generate user description from user's title and skills
    user_text = preprocess_text(f"{user_data['Title']} {user_data['CombinedSkills']} {user_data['Address']}")

    # Maintain a list of recommended jobs
    recommended_jobs = []

    # Iterate through jobs and compute similarity
    for job in jobs:
        job_id = job['id']
        job_title = job['jobTitle']
        job_skills_list = job['skills']
        job_skills_text = ', '.join(job_skills_list)

        # Generate job description from job title and skills
        job_description = ', '.join(job['descriptions'])

        job_text = preprocess_text(f"{job_title} {job_skills_text} {job_description}")

        # Compute similarity
        if job_skills_list:  # Only compute similarity if job has skills
            similarity = compute_similarity(user_text, job_text)
            user_skills_set = set(user_data['CombinedSkills'].split(', '))
            job_skills_set = set(job_skills_list)
            if user_skills_set.issuperset(job_skills_set):
                similarity = 1.0  # Set similarity to maximum if user has all required skills
        else:
            # If job has no skills, compute similarity based on job title and description only
            similarity = compute_similarity(user_text, preprocess_text(f"{job_title} {job_description}"))

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
