pipeline {
    agent any

    environment {
        DOTNET_ROOT = "/usr/share/dotnet"
        PATH = "$DOTNET_ROOT:$PATH"
    }

    stages {
        stage('Checkout') {
            steps {
                echo 'Checking out source code...'
                checkout scm
            }
        }

        stage('Restore Dependencies') {
            steps {
                echo 'Restoring dependencies...'
                sh 'dotnet restore CleanArchitecture.sln'
            }
        }

        stage('Build') {
            steps {
                echo 'Building the project...'
                sh 'dotnet build CleanArchitecture.sln --configuration Release --no-restore'
            }
        }

        stage('Test') {
            steps {
                echo 'Running unit tests...'
                sh 'dotnet test CleanArchitecture.sln --configuration Release --no-build --verbosity normal'
            }
        }

        stage('Docker Build') {
            steps {
                echo 'Building Docker image...'
                sh 'docker build -t cleanarchitecture:latest -f Dockerfile .'
            }
        }

        stage('Push to Registry') {
            steps {
                script {
                    echo 'Pushing Docker image to DockerHub or private registry...'
                    withCredentials([string(credentialsId: 'dockerhub-password', variable: 'DOCKER_PASSWORD')]) {
                        sh '''
                        // echo $DOCKER_PASSWORD | docker login -u "your-dockerhub-username" --password-stdin
                        // docker tag cleanarchitecture:latest your-dockerhub-username/cleanarchitecture:latest
                        // docker push your-dockerhub-username/cleanarchitecture:latest
                        '''
                    }
                }
            }
        }

        stage('Deploy to Kubernetes') {
            steps {
                echo 'Deploying to Kubernetes cluster...'
                sh '''
                kubectl apply -f k8s-deployments/
                kubectl rollout status deployment/clean-architecture
                '''
            }
        }
    }

    post {
        success {
            echo 'Build, Test, Dockerize, and Deployment succeeded!'
        }
        failure {
            echo 'Pipeline failed. Check logs for errors.'
        }
    }
}
