import os
import shutil
from collections import defaultdict
from flask import Flask, request, jsonify
from nltk.corpus import stopwords
from nltk.tokenize import word_tokenize
from nltk.stem import WordNetLemmatizer
from docx import Document
from transformers import BertTokenizer, BertModel
from fuzzywuzzy import fuzz
import nltk
nltk.download('wordnet')

app = Flask(__name__)

# Initialize BERT tokenizer and model
tokenizer = BertTokenizer.from_pretrained('bert-base-uncased')
bert_model = BertModel.from_pretrained('bert-base-uncased')

# Storage for matched CVs
matched_cvs_storage = {}

# Function to preprocess CV text
def preprocess_cv(cv_text):
    stop_words = set(stopwords.words('english'))
    lemmatizer = WordNetLemmatizer()
    skills = []
    contact_info = {}

    for line in cv_text:
        words = word_tokenize(line)
        words = [word.lower() for word in words if word.isalpha() and word.lower() not in stop_words]
        words = [lemmatizer.lemmatize(word) for word in words]

        if line.startswith("3."):
            skill_name = line.split(":")[1].split('(')[0].strip()  # Extract SkillName without SkillLevel
            skills.append(skill_name)
        elif line.lower().startswith("full name:"):
            contact_info['full_name'] = line.split(":")[1].strip()
        elif line.lower().startswith("address:"):
            contact_info['address'] = line.split(":")[1].strip()
        elif line.lower().startswith("phone number:"):
            contact_info['phone_number'] = line.split(":")[1].strip()
        elif line.lower().startswith("email:"):
            contact_info['email'] = line.split(":")[1].strip()

    return skills, contact_info

# Function to load CV data from folder
def load_cv_data(folder_path):
    file_names = os.listdir(folder_path)
    cv_data = []
    for file_name in file_names:
        if file_name.endswith('.docx'):
            file_path = os.path.join(folder_path, file_name)
            doc = Document(file_path)
            cv_text = [paragraph.text for paragraph in doc.paragraphs if paragraph.text.strip()]
            skills, contact_info = preprocess_cv(cv_text)
            cv_data.append((skills, contact_info, file_name, file_path))
    return cv_data

# Function to detect similarity based on entered skills
def detect_similarity(entered_skills, cv_data):
    entered_skills = [skill.lower() for skill in entered_skills]  # Convert entered skills to lowercase

    cv_matches = defaultdict(list)

    for skills, contact_info, file_name, file_path in cv_data:
        cv_skills = [skill.lower() for skill in skills]  # Lower case the skills from CV

        common_skills = []
        for entered_skill in entered_skills:
            for cv_skill in cv_skills:
                if fuzz.partial_ratio(entered_skill, cv_skill) >= 80:  # Adjust matching ratio as needed
                    common_skills.append(cv_skill)
                    break  # Move to the next entered skill once a match is found

        match_score = len(common_skills) / len(entered_skills)  # Score based on overlap with entered skills

        cv_matches[match_score].append((contact_info, file_name, file_path, common_skills))

    sorted_scores = sorted(cv_matches.keys(), reverse=True)
    recommended_cvs = []

    for score in sorted_scores:
        if score > 0:
            for cv_info in cv_matches[score]:
                recommended_cvs.append(cv_info)

    return recommended_cvs

# Endpoint to get matching CVs based on skills
@app.route('/match-cvs', methods=['POST'])
def match_cvs():
    data = request.json
    folder_path = data.get('folder_path')
    skills = data.get('skills')

    if not folder_path or not skills:
        return jsonify({"error": "Folder path and skills are required"}), 400

    cv_data = load_cv_data(folder_path)
    if not cv_data:
        return jsonify({"error": "No CV data found in the selected folder"}), 404

    similar_cvs = detect_similarity(skills, cv_data)
    if not similar_cvs:
        return jsonify({"message": "No CVs found matching the entered skills"}), 404

    # Store the results in the global storage
    matched_cvs_storage['cvs'] = similar_cvs

    results = []
    for cv_info in similar_cvs:
        contact_info = cv_info[0]
        recommended_cv_file = cv_info[1]
        recommended_cv_path = cv_info[2]
        matched_skills = cv_info[3]

        results.append({
            "file_name": recommended_cv_file,
            "file_path": recommended_cv_path,
            "contact_info": {
                "email": contact_info.get('email', 'N/A'),
                "phone_number": contact_info.get('phone_number', 'N/A')
            },
            "matched_skills": matched_skills
        })

    return jsonify(results)

# Endpoint to get the matched CVs
@app.route('/get-matched-cvs', methods=['GET'])
def get_matched_cvs():
    if 'cvs' not in matched_cvs_storage or not matched_cvs_storage['cvs']:
        return jsonify({"error": "No matched CVs found. Please run the match-cvs endpoint first."}), 400

    results = []
    for cv_info in matched_cvs_storage['cvs']:
        contact_info = cv_info[0]
        recommended_cv_file = cv_info[1]
        recommended_cv_path = cv_info[2]
        matched_skills = cv_info[3]

        results.append({
            "file_name": recommended_cv_file,
            "file_path": recommended_cv_path,
            "contact_info": {
                "email": contact_info.get('email', 'N/A'),
                "phone_number": contact_info.get('phone_number', 'N/A')
            },
            "matched_skills": matched_skills
        })

    return jsonify(results)

# Endpoint to download selected CVs
@app.route('/download-cvs', methods=['POST'])
def download_cvs():
    data = request.json
    destination_folder = data.get('destination_folder')

    if 'cvs' not in matched_cvs_storage or not matched_cvs_storage['cvs']:
        return jsonify({"error": "No CVs found to download. Please run the match-cvs endpoint first."}), 400

    if not destination_folder:
        return jsonify({"error": "Destination folder is required"}), 400

    if not os.path.exists(destination_folder):
        os.makedirs(destination_folder, exist_ok=True)

    downloaded_files = []
    for cv_info in matched_cvs_storage['cvs']:
        recommended_cv_path = cv_info[2]  # Get the CV path
        downloaded_cv_path = copy_or_move_file(recommended_cv_path, destination_folder, move=True)
        downloaded_files.append(downloaded_cv_path)

    return jsonify({"message": "CVs downloaded successfully", "downloaded_files": downloaded_files})

# Function to copy or move file to destination
def copy_or_move_file(src_path, dest_folder, move=False):
    file_name = os.path.basename(src_path)
    dest_path = os.path.join(dest_folder, file_name)
    if move:
        shutil.move(src_path, dest_path)
    else:
        shutil.copy(src_path, dest_path)
    return dest_path

if __name__ == '__main__':
    app.run(debug=True)
