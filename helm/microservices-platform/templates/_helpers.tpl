{{/* Common labels */}}
{{- define "micro.name" -}}
{{- default .Chart.Name .Values.nameOverride -}}
{{- end -}}

{{- define "micro.labels" -}}
app.kubernetes.io/name: {{ include "micro.name" . }}
helm.sh/chart: {{ .Chart.Name }}-{{ .Chart.Version }}
app.kubernetes.io/managed-by: Helm
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/version: {{ .Chart.AppVersion }}
{{- end -}}
